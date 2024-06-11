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

using QuickLook.Common.Plugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace QuickLook.Plugin.ExecutableViewer;

public class Plugin : IViewer
{
    private static readonly HashSet<string> hashSet =
    [
        ".exe", ".sys", ".scr", ".ocx", ".cpl", ".bpl",
        ".dll", ".ax", ".drv", ".vxd",
        ".mui",
        ".tlb",
        ".msi",
        ".efi", ".mz",
    ];

    private static readonly HashSet<string> WellKnownImageExtensions = hashSet;

    private InfoPanel? _ip;

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
        context.PreferredSize = new Size { Width = 453, Height = 172 };
    }

    public void View(string path, ContextObject context)
    {
        _path = path;
        _ip = new InfoPanel();

        // https://learn.microsoft.com/zh-cn/windows/win32/debug/pe-format
        string arch = HeaderReader.GetArchitecture(_path!);
        _ip.DisplayInfo(_path);
        _ip.DisplayArch(arch);

        _ip.Tag = context;

        context.ViewerContent = _ip;
        context.Title = $"{Path.GetFileName(path)}";
        context.IsBusy = false;
    }

    public void Cleanup()
    {
        GC.SuppressFinalize(this);

        _ip = null;
    }
}
