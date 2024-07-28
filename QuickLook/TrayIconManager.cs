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

using QuickLook.Common.Helpers;
using QuickLook.Helpers;
using QuickLook.Properties;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using NotifyIcon = NotifyIconEx.NotifyIcon;

namespace QuickLook;

internal class TrayIconManager : IDisposable
{
    private static TrayIconManager _instance = null!;

    private readonly NotifyIcon _icon;

    private readonly ToolStripMenuItem _itemAutorun = null!;

    private TrayIconManager()
    {
        _icon = new NotifyIcon
        {
            Text = string.Format(TranslationHelper.Get("Icon_ToolTip"),
                Application.ProductVersion),
            Icon = GetTrayIconByDPI(),
            Visible = SettingHelper.Get("ShowTrayIcon", true)
        };

        _icon.AddMenu($"v{Application.ProductVersion.Split('+')[0]}{(App.IsUWP ? " UWP" : "")} (r3)").Enabled = false;
        _icon.AddMenu("-");
        _icon.AddMenu(TranslationHelper.Get("Icon_CheckUpdate"), (_, _) => Updater.CheckForUpdates());
        _icon.AddMenu(TranslationHelper.Get("Icon_InstalledPlugin"), (_, _) => Process.Start($"file://{App.UserPluginPath}"));
        _icon.AddMenu(TranslationHelper.Get("Icon_GetPlugin"),
                (_, _) => Process.Start("https://github.com/QL-Win/QuickLook/wiki/Available-Plugins"));
        _itemAutorun = _icon.AddMenu(TranslationHelper.Get("Icon_RunAtStartup"),
            (sender, e) =>
            {
                if (AutoStartupHelper.IsAutorun())
                    AutoStartupHelper.RemoveAutorunShortcut();
                else
                    AutoStartupHelper.CreateAutorunShortcut();
            }) as ToolStripMenuItem;
        _itemAutorun.Enabled = !App.IsUWP;
        _icon.AddMenu(TranslationHelper.Get("Icon_Restart"),
            (_, _) => Restart(forced: true));
        _icon.AddMenu(TranslationHelper.Get("Icon_Quit"),
            (_, _) => System.Windows.Application.Current.Shutdown());

        _icon.ContextMenuStrip.Opened += (sender, e) => { _itemAutorun.Checked = AutoStartupHelper.IsAutorun(); };
    }

    public void Dispose()
    {
        _icon.Visible = false;
    }

    public void Restart(string fileName = null!, string dir = null!, string args = null!, int? exitCode = null, bool forced = false)
    {
        _ = args;

        try
        {
            Process process = new()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = fileName ?? Path.Combine(dir ?? AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.FriendlyName),
                    WorkingDirectory = dir ?? Environment.CurrentDirectory,
                    UseShellExecute = true,
                },
            };
            process.Start();
        }
        catch (Win32Exception)
        {
            return;
        }
        if (forced)
        {
            Process.GetCurrentProcess().Kill();
        }
        Environment.Exit(exitCode ?? 'r' + 'e' + 's' + 't' + 'a' + 'r' + 't');
    }

    private Icon GetTrayIconByDPI()
    {
        var scale = DisplayDeviceHelper.GetCurrentScaleFactor().Vertical;

        if (!App.IsWin10)
            return scale > 1 ? Resources.app : Resources.app_16;

        return OSThemeHelper.SystemUsesDarkTheme()
            ? (scale > 1 ? Resources.app_white : Resources.app_white_16)
            : (scale > 1 ? Resources.app_black : Resources.app_black_16);
    }

    public static void ShowNotification(string title, string content, bool isError = false, int timeout = 5000,
        Action clickEvent = null!,
        Action closeEvent = null!)
    {
        var icon = GetInstance()._icon;
        icon.ShowBalloonTip(timeout, title, content, isError ? ToolTipIcon.Error : ToolTipIcon.Info);
        icon.BalloonTipClicked += OnIconOnBalloonTipClicked;
        icon.BalloonTipClosed += OnIconOnBalloonTipClosed;

        void OnIconOnBalloonTipClicked(object sender, EventArgs e)
        {
            clickEvent?.Invoke();
            icon.BalloonTipClicked -= OnIconOnBalloonTipClicked;
        }

        void OnIconOnBalloonTipClosed(object sender, EventArgs e)
        {
            closeEvent?.Invoke();
            icon.BalloonTipClosed -= OnIconOnBalloonTipClosed;
        }
    }

    public static TrayIconManager GetInstance()
    {
        return _instance ??= new TrayIconManager();
    }
}
