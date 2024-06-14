// Copyright © 2018 Jeremy Hart
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
using System.Linq;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Globalization;
using QuickLook.Common.Helpers;
using System.Windows;

namespace QuickLook.Plugin.FontViewer;

public partial class FontViewerPanel : UserControl
{
    public FontViewerPanel()
    {
        InitializeComponent();

        var testString = TranslationHelper.Get("SAMPLE_TEXT");

        Thickness marginbottom = new(0, 0, 0, 5);

        var alpha = new Paragraph();
        alpha.Inlines.Add(new Run(TranslationHelper.Get("ALPHA")));
        alpha.Margin = marginbottom;
        RichTB.Document.Blocks.Add(alpha);

        var numeric = new Paragraph();
        numeric.Inlines.Add(new Run(TranslationHelper.Get("DIGITS") + TranslationHelper.Get("PUNCTUATION") + TranslationHelper.Get("SYMBOLS")));
        numeric.Margin = marginbottom;
        RichTB.Document.Blocks.Add(numeric);

        var size12 = new Paragraph();
        size12.Inlines.Add(new Run(string.Format(testString, Environment.TickCount)));
        size12.Margin = marginbottom;
        size12.FontSize = 12;
        RichTB.Document.Blocks.Add(size12);

        var size18 = new Paragraph();
        size18.Inlines.Add(new Run(string.Format(testString, Environment.TickCount)));
        size18.FontSize = 18;
        size18.Margin = marginbottom;
        RichTB.Document.Blocks.Add(size18);

        var size24 = new Paragraph();
        size24.Inlines.Add(new Run(string.Format(testString, Environment.TickCount)));
        size24.FontSize = 24;
        size24.Margin = marginbottom;
        RichTB.Document.Blocks.Add(size24);

        var size36 = new Paragraph();
        size36.Inlines.Add(new Run(string.Format(testString, Environment.TickCount)));
        size36.FontSize = 36;
        size36.Margin = marginbottom;
        RichTB.Document.Blocks.Add(size36);

        var size48 = new Paragraph();
        size48.Inlines.Add(new Run(string.Format(testString, Environment.TickCount)));
        size48.FontSize = 48;
        size48.Margin = marginbottom;
        RichTB.Document.Blocks.Add(size48);

        var size60 = new Paragraph();
        size60.Inlines.Add(new Run(string.Format(testString, Environment.TickCount)));
        size60.FontSize = 60;
        size60.Margin = marginbottom;
        RichTB.Document.Blocks.Add(size60);
    }

    public void LoadFont(string path)
    {
        var a = Fonts.GetFontFamilies(path);

        GlyphTypeface gf = new(new Uri(path));

        string fontFamilyName = gf.FamilyNames[CultureInfo.CurrentCulture] ?? gf.FamilyNames.Values.FirstOrDefault();
        string fontFaceName = gf.FaceNames[CultureInfo.CurrentCulture] ?? gf.FaceNames.Values.FirstOrDefault();
        string manufacturer = gf.ManufacturerNames[CultureInfo.CurrentCulture] ?? gf.ManufacturerNames.Values.FirstOrDefault();
        string copyright = gf.Copyrights[CultureInfo.CurrentCulture] ?? gf.Copyrights.Values.FirstOrDefault();

        var fontName = new Paragraph();
        fontName.Inlines.Add(new Run(string.Format(fontFamilyName + " " + fontFaceName, Environment.TickCount)));
        fontName.FontSize = 45;
        fontName.FontFamily = a.ToArray()[0];
        fontName.Margin = new Thickness(0, 0, 0, 0);
        RichTBDetails.Document.Blocks.Add(fontName);

        if (manufacturer != null)
        {
            var foundry = new Paragraph();
            foundry.Inlines.Add(new Run(string.Format("by " + manufacturer, Environment.TickCount)));
            foundry.FontSize = 14;
            foundry.Margin = new Thickness(0, 0, 0, 0);
            RichTBDetails.Document.Blocks.Add(foundry);
        }

        if (copyright != null)
        {
            var copyR = new Paragraph();
            copyR.Inlines.Add(new Run(string.Format(copyright, Environment.TickCount)));
            copyR.FontSize = 14;
            copyR.Margin = new Thickness(0, 0, 0, 0);
            RichTBDetails.Document.Blocks.Add(copyR);
        }

        RichTB.FontFamily = a.ToArray()[0];
    }
}
