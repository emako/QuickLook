﻿// Copyright © 2024 ema
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

using MediaInfoLib;
using QuickLook.Common.Helpers;
using QuickLook.Common.Plugin;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace QuickLook.Plugin.MediaInfoViewer;

public class Plugin : IViewer
{
    private TextViewerPanel _tvp;

    public int Priority => 0;

    public void Init()
    {
    }

    public bool CanHandle(string path)
    {
        return false;

#pragma warning disable CS0162 // Unreachable code detected
        using MediaInfo lib = new MediaInfo().WithOpen(path);
#pragma warning restore CS0162 // Unreachable code detected
        return Enum.GetValues(typeof(StreamKind))
            .Cast<StreamKind>()
            .Where(v => v != StreamKind.General)
            .Any(v => lib.Count_Get(v) > 0);
    }

    public void Prepare(string path, ContextObject context)
    {
        context.PreferredSize = new Size { Width = 800, Height = 600 };
    }

    public void View(string path, ContextObject context)
    {
        using MediaInfo lib = new MediaInfo()
            .WithOpen(path);

        _tvp = new TextViewerPanel(lib.Inform(), context);
        AssignHighlightingManager(_tvp, context);

        _tvp.Tag = context;
        _tvp.Drop += OnDrop;

        context.ViewerContent = _tvp;
        context.Title = $"{Path.GetFileName(path)}";
        context.IsBusy = false;
    }

    private void OnDrop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            if (e.Data.GetData(DataFormats.FileDrop) is string[] files
                && files.FirstOrDefault() is string path)
            {
                if (_tvp.Tag is ContextObject context)
                {
                    context.Title = $"{Path.GetFileName(path)}";
                }

                using MediaInfo lib = new MediaInfo()
                    .WithOpen(path);
                _tvp.Text = lib.Inform();
            }
        }
    }

    public void Cleanup()
    {
        GC.SuppressFinalize(this);

        _tvp = null;
    }

    private void AssignHighlightingManager(TextViewerPanel tvp, ContextObject context)
    {
        var darkThemeAllowed = SettingHelper.Get("AllowDarkTheme", false, "QuickLook.Plugin.MediaInfoViewer");
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
