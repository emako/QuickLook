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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace QuickLook.Plugin.FontViewer;

public class Plugin : IViewer
{
    private FontViewerPanel? _fontPanel;

    public int Priority => int.MaxValue;

    public void Init()
    {
    }

    public bool CanHandle(string path)
    {
        return !Directory.Exists(path) && new string[] { ".ttf", ".otf" }.Any(path.ToLower().EndsWith);
    }

    public void Prepare(string path, ContextObject context)
    {
        context.PreferredSize = new Size { Width = 1300, Height = 500 };
    }

    public void View(string path, ContextObject context)
    {
        GlyphTypeface gf = new(new Uri(path));

        string fontFamilyName = gf.FamilyNames[CultureInfo.CurrentCulture] ?? gf.FamilyNames.Values.FirstOrDefault();
        string fontFaceName = gf.FaceNames[CultureInfo.CurrentCulture] ?? gf.FaceNames.Values.FirstOrDefault();

        _fontPanel = new FontViewerPanel();
        context.ViewerContent = _fontPanel;
        context.Title = fontFamilyName + " " + fontFaceName;

        _fontPanel.LoadFont(path);
        AssignHighlightingManager(_fontPanel, context);

        context.IsBusy = false;
    }

    public void Cleanup()
    {
        GC.SuppressFinalize(this);
    }

    private void AssignHighlightingManager(FontViewerPanel fontPanel, ContextObject context)
    {
        var darkThemeAllowed = SettingHelper.Get("AllowDarkTheme", false, "QuickLook.Plugin.FontViewer");
        var isDark = darkThemeAllowed && OSThemeHelper.AppsUseDarkTheme();

        if (isDark)
        {
            fontPanel.Foreground = new BrushConverter().ConvertFromString("#FFEFEFEF") as SolidColorBrush;
            fontPanel.Background = Brushes.Transparent;
        }
        else
        {
            fontPanel.Foreground = new BrushConverter().ConvertFromString("#BBFAFAFA") as SolidColorBrush;
            fontPanel.Background = Brushes.Transparent;
        }
    }
}
