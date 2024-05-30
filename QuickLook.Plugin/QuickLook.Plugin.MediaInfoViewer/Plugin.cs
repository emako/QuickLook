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

using MediaInfoLib;
using QuickLook.Common.Plugin;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace QuickLook.Plugin.CsvViewer;

public class Plugin : IViewer
{
    private TextBox _panel;

    public int Priority => 0;

    public void Init()
    {
    }

    public bool CanHandle(string path)
    {
        return false;

        using MediaInfo lib = new MediaInfo().WithOpen(path);
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

        _panel = new TextBox()
        {
            Text = lib.Inform(),
            FontFamily = new System.Windows.Media.FontFamily("Consolas"),
        };

        context.ViewerContent = _panel;
        context.Title = $"{Path.GetFileName(path)}";
        context.IsBusy = false;
    }

    public void Cleanup()
    {
        GC.SuppressFinalize(this);

        _panel = null;
    }
}
