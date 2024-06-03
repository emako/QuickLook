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
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace QuickLook.Plugin.BinaryViewer;

public class Plugin : IViewer
{
    private BinaryViewerPanel _bvp;
    private string _path;

    public int Priority => 0;

    public void Init()
    {
    }

    public bool CanHandle(string path)
    {
        return false;
    }

    public void Prepare(string path, ContextObject context)
    {
        context.PreferredSize = new Size { Width = 800, Height = 600 };
    }

    public void View(string path, ContextObject context)
    {
        _path = path;
        _bvp = new BinaryViewerPanel();
        AssignHighlightingManager(_bvp, context);

        _ = Task.Run(async () =>
        {
            string text = await LoadBinaryAsync(BinaryType.HEX);
            _bvp.LoadTextAsync(text);
            context.IsBusy = false;
        });

        _bvp.Tag = context;
        _bvp.PropertyChanged += async (sender, e) =>
        {
            if (e.PropertyName == nameof(BinaryViewerPanel.BinaryType))
            {
                context.IsBusy = true;
                string lines = await LoadBinaryAsync(_bvp.BinaryType);
                _bvp.LoadTextAsync(lines);
                context.IsBusy = false;
            }
        };

        context.ViewerContent = _bvp;
        context.Title = $"{Path.GetFileName(path)}";
    }

    public void Cleanup()
    {
        GC.SuppressFinalize(this);

        _bvp = null;
    }

    public async Task<string> LoadBinaryAsync(BinaryType type)
    {
        if (!File.Exists(_path))
        {
            return "ERR: File Not Found.";
        }

        try
        {
            byte[] buffer = new byte[100000];

            using FileStream fs = new(_path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);
            int bytesRead = await fs.ReadAsync(buffer, 0, buffer.Length);

            if (bytesRead < buffer.Length)
            {
                Array.Resize(ref buffer, bytesRead);
            }

            if (type == BinaryType.HEX)
            {
                return BinaryConverter.ToHexString(buffer, 0, buffer.Length, sep: ' ', lineLength: 20);
            }
            else if (type == BinaryType.BYTE)
            {
                return BinaryConverter.ToByteString(buffer, 0, buffer.Length, sep: ' ', lineLength: 20);
            }
            else if (type == BinaryType.CHAR)
            {
                return BinaryConverter.ToCharString(buffer, 0, buffer.Length, sep: ' ', lineLength: 20);
            }
        }
        catch (Exception e)
        {
            return $"ERR: ${e.Message}";
        }

        return string.Empty;
    }

    private void AssignHighlightingManager(BinaryViewerPanel bvp, ContextObject context)
    {
        var darkThemeAllowed = SettingHelper.Get("AllowDarkTheme", false, "QuickLook.Plugin.MediaInfoViewer");
        var isDark = darkThemeAllowed && OSThemeHelper.AppsUseDarkTheme();

        if (isDark)
        {
            bvp.Foreground = new BrushConverter().ConvertFromString("#FFEFEFEF") as SolidColorBrush;
        }
        else
        {
            bvp.Foreground = new BrushConverter().ConvertFromString("#BBFAFAFA") as SolidColorBrush;
        }
    }
}
