// Copyright © 2024 ema
//
// This file is part of QuickLook program.
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using QuickLook.Common.Helpers;
using QuickLook.Common.Plugin;
using QuickLook.Plugin.HashViewer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace QuickLook.Plugin.BinaryViewer;

public class Plugin : IViewer
{
    private static readonly HashSet<string> hashSet =
    [
        ".exe", ".sys", ".scr", ".ocx", ".cpl", ".bpl",
        ".dll", ".ax", ".drv", ".vxd",
        ".mui",
        ".pdb",
        ".tlb",
        ".msi",
        ".efi", ".mz",
    ];

    private static readonly HashSet<string> WellKnownImageExtensions = hashSet;

    private TextViewerPanel? _tvp;
    private string? _path;

    public int Priority => 0;

    public void Init()
    {
    }

    public bool CanHandle(string path)
    {
        return WellKnownImageExtensions.Contains(Path.GetExtension(path.ToLower()));
    }

    public void Prepare(string path, ContextObject context)
    {
        context.PreferredSize = new Size { Width = 800, Height = 600 };
    }

    public void View(string path, ContextObject context)
    {
        _path = path;
        _tvp = new TextViewerPanel();
        AssignHighlightingManager(_tvp, context);

        _ = Task.Run(() =>
        {
            string text = LoadHash();
            _tvp.LoadTextAsync(text);
            context.IsBusy = false;
        });

        _tvp.Tag = context;

        context.ViewerContent = _tvp;
        context.Title = $"{Path.GetFileName(path)}";
    }

    public void Cleanup()
    {
        GC.SuppressFinalize(this);

        _tvp = null;
    }

    public string LoadHash()
    {
        if (!File.Exists(_path))
        {
            return "ERR: File Not Found.";
        }

        try
        {
            // https://learn.microsoft.com/zh-cn/windows/win32/debug/pe-format
            return HeaderReader.GetHeaders(_path!);
        }
        catch (Exception e)
        {
            return $"ERR: ${e.Message}";
        }
    }

    private void AssignHighlightingManager(TextViewerPanel tvp, ContextObject context)
    {
        var darkThemeAllowed = SettingHelper.Get("AllowDarkTheme", false, "QuickLook.Plugin.HashViewer");
        var isDark = darkThemeAllowed && OSThemeHelper.AppsUseDarkTheme();

        if (isDark)
        {
            tvp.Foreground = new BrushConverter().ConvertFromString("#FFEFEFEF") as SolidColorBrush;
            tvp.Background = Brushes.Transparent;
        }
        else
        {
            tvp.Foreground = new BrushConverter().ConvertFromString("#BBFAFAFA") as SolidColorBrush;
            tvp.Background = Brushes.Transparent;
        }
    }
}
