// Copyright © 2024 ema
//
// This file a plugin for the QuickLook program.
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
// along with this program.  If not, see <http://www.gnu.org/licenses/>

using System;
using System.Windows.Controls;
using System.Windows.Media;

namespace QuickLook.Plugin.FontViewer;

public partial class IconFontViewerPanel : UserControl
{
    public IconFontViewerPanel()
    {
        InitializeComponent();
    }

    public void LoadFont(string path)
    {
        var a = Fonts.GetFontFamilies(path);

        GlyphTypeface gf = new(new Uri(path));
    }
}
