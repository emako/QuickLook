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

using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.AvalonEdit.Search;
using QuickLook.Common.Helpers;
using QuickLook.Common.Plugin;
using System;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace QuickLook.Plugin.MediaInfoViewer;

public class TextViewerPanel : TextEditor, IDisposable
{
    private readonly ContextObject _context;
    private bool _disposed;
    private HighlightingManager highlightingManager = HighlightingManager.Instance;
    internal string? _text;

    public TextViewerPanel(string text, ContextObject context)
    {
        _context = context;

        Margin = new Thickness(8, 0, 0, 0);
        FontSize = 14;
        ShowLineNumbers = false;
        WordWrap = true;
        IsReadOnly = true;
        IsManipulationEnabled = true;
        Options.EnableEmailHyperlinks = false;
        Options.EnableHyperlinks = false;

        ContextMenu = new ContextMenu();
        ContextMenu.Items.Add(new MenuItem
        {
            Header = TranslationHelper.Get("Editor_Copy", domain: Assembly.GetExecutingAssembly().GetName().Name),
            Command = ApplicationCommands.Copy
        });
        ContextMenu.Items.Add(new MenuItem
        {
            Header = TranslationHelper.Get("Editor_SelectAll",
                domain: Assembly.GetExecutingAssembly().GetName().Name),
            Command = ApplicationCommands.SelectAll
        });

        ManipulationInertiaStarting += Viewer_ManipulationInertiaStarting;
        ManipulationStarting += Viewer_ManipulationStarting;
        ManipulationDelta += Viewer_ManipulationDelta;

        PreviewMouseWheel += Viewer_MouseWheel;

        FontFamily = new FontFamily("Consolas, " + TranslationHelper.Get("Editor_FontFamily",
            domain: Assembly.GetExecutingAssembly().GetName().Name));

        TextArea.TextView.ElementGenerators.Add(new TruncateLongLines());

        SearchPanel.Install(this);

        LoadTextAsync(text);
    }

    public HighlightingManager HighlightingManager
    {
        get => highlightingManager;
        set => highlightingManager = value;
    }

    public void Dispose()
    {
        _disposed = true;
    }

    private void Viewer_ManipulationInertiaStarting(object sender, ManipulationInertiaStartingEventArgs e)
    {
        e.TranslationBehavior = new InertiaTranslationBehavior
        {
            InitialVelocity = e.InitialVelocities.LinearVelocity,
            DesiredDeceleration = 10.0 * 96.0 / (1000.0 * 1000.0)
        };
    }

    private void Viewer_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        e.Handled = true;

        ScrollToVerticalOffset(VerticalOffset - e.Delta);
    }

    private void Viewer_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
    {
        e.Handled = true;

        var delta = e.DeltaManipulation;
        ScrollToVerticalOffset(VerticalOffset - delta.Translation.Y);
    }

    private void Viewer_ManipulationStarting(object sender, ManipulationStartingEventArgs e)
    {
        e.Mode = ManipulationModes.Translate;
    }

    private class TruncateLongLines : VisualLineElementGenerator
    {
        private const int MAX_LENGTH = 10000;
        private const string ELLIPSIS = "⁞⁞[TRUNCATED]⁞⁞";

        public override int GetFirstInterestedOffset(int startOffset)
        {
            var line = CurrentContext.VisualLine.LastDocumentLine;
            if (line.Length > MAX_LENGTH)
            {
                int ellipsisOffset = line.Offset + MAX_LENGTH - ELLIPSIS.Length;
                if (startOffset <= ellipsisOffset)
                    return ellipsisOffset;
            }
            return -1;
        }

        public override VisualLineElement ConstructElement(int offset)
        {
            return new FormattedTextElement(ELLIPSIS, CurrentContext.VisualLine.LastDocumentLine.EndOffset - offset);
        }
    }

    private void LoadTextAsync(string text)
    {
        _text = text;

        Task.Run(() =>
        {
            if (_disposed)
                return;

            var doc = new TextDocument(_text);
            doc.SetOwnerThread(Dispatcher.Thread);

            if (_disposed)
                return;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                Encoding = Encoding.UTF8;
                Document = doc;

                _context.IsBusy = false;
            }), DispatcherPriority.Render);
        });
    }
}
