// Copyright © 2017 Paddy Xu
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

using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Controls;
using QuickLook.Common.ExtensionMethods;
using QuickLook.Common.Helpers;

namespace QuickLook.Plugin.ExecutableViewer;

public partial class InfoPanel : UserControl
{
    private bool _stop;

    public InfoPanel()
    {
        InitializeComponent();

        // apply global theme
        Resources.MergedDictionaries[0].Clear();
    }

    public bool Stop
    {
        set => _stop = value;
        get => _stop;
    }

    public void DisplayInfo(string path)
    {
        Task.Run(() =>
        {
            var scale = DisplayDeviceHelper.GetCurrentScaleFactor();

            var icon =
                WindowsThumbnailProvider.GetThumbnail(path,
                    (int)(128 * scale.Horizontal),
                    (int)(128 * scale.Vertical),
                    ThumbnailOptions.ScaleUp);

            var source = icon?.ToBitmapSource();
            icon?.Dispose();

            Dispatcher.BeginInvoke(new Action(() => image.Source = source));
        });

        var name = Path.GetFileName(path);
        filename.Text = string.IsNullOrEmpty(name) ? path : name;

        var last = File.GetLastWriteTime(path);
        modDate.Text = string.Format(TranslationHelper.Get("InfoPanel_LastModified"),
            last.ToString(CultureInfo.CurrentCulture));

        Stop = false;

        Task.Run(() =>
        {
            if (File.Exists(path))
            {
                var size = new FileInfo(path).Length;

                Dispatcher.Invoke(() => { totalSize.Text = size.ToPrettySize(2); });
            }
        });
    }

    public void DisplayArch(string arch)
    {
        Dispatcher.Invoke(() => { architecture.Text = arch; });
    }
}
