using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using Docdown.Util;
using Docdown.ViewModel;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;

namespace Docdown.Editor
{
    public partial class DefaultEditor : UserControl, IEditor
    {
        public TextEditor Editor => EditBox;

        private CompletionWindow completionWindow;
        private readonly ToolTip toolTip = new ToolTip();

        public DefaultEditor()
        {
            InitializeComponent();

            Editor.Options.IndentationSize = 4;
            Editor.Options.ConvertTabsToSpaces = true;
            Editor.Options.AllowScrollBelowDocument = true;
            Editor.Options.EnableHyperlinks = false;
            Editor.Options.EnableEmailHyperlinks = false;
            Editor.Options.EnableRectangularSelection = false;

            Editor.TextArea.TextEntered += TextEntered;
            Editor.TextArea.TextEntering += TextEntering;
            Editor.TextArea.PreviewKeyDown += ShowCompletionWindowKeyboard;

            Editor.MouseHover += TextEditorMouseHover;
            Editor.MouseHoverStopped += TextEditorMouseHoverStopped;
        }

        void TextEditorMouseHover(object sender, MouseEventArgs e)
        {
            var pos = Editor.GetPositionFromPoint(e.GetPosition(Editor));
            if (pos != null && DataContext is WorkspaceItemViewModel item && item.Editor != null)
            {
                int index = Editor.Document.GetOffset(pos.Value.Line, pos.Value.Column);
                var editor = item.Editor;
                var content = editor.FindHoverContent(index);
                if (content != null)
                {
                    toolTip.PlacementTarget = this;
                    toolTip.Content = content;
                    toolTip.IsOpen = true;
                    e.Handled = true;
                }
            }
        }

        void TextEditorMouseHoverStopped(object sender, MouseEventArgs e)
        {
            toolTip.IsOpen = false;
        }

        private void ShowCompletionWindowKeyboard(object sender, KeyEventArgs e)
        {
            if (completionWindow == null && e.Key == Key.Space && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                ShowCompletionWindow();
                e.Handled = true;
            }
        }

        private void TextEntered(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length == 1 && DataContext is WorkspaceItemViewModel item && item.Editor != null)
            {
                var c = e.Text[0];
                if (item.Editor.CompletionMarkers != null && item.Editor.CompletionMarkers.Contains(c))
                {
                    ShowCompletionWindow();
                }
            }
        }

        void TextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0 && completionWindow != null && completionWindow.CompletionList.IsEmpty())
            {
                completionWindow.Close();
            }
        }

        private void ShowCompletionWindow()
        {
            if (completionWindow == null && DataContext is WorkspaceItemViewModel item && item.Editor != null)
            {
                completionWindow = new CompletionWindow(Editor.TextArea);
                var editor = item.Editor;
                if (editor.FillCompletionList(completionWindow.CompletionList, Editor.SelectionStart))
                {
                    completionWindow.Show();
                    completionWindow.Closed += delegate
                    {
                        completionWindow = null;
                    };
                }
                else
                {
                    completionWindow = null;
                }
            }
        }

        private void SearchControl_IsVisibleChanged(object _1, System.Windows.DependencyPropertyChangedEventArgs _2)
        {
            if (Search.IsVisible)
            {
                FocusManager.SetFocusedElement(this, Search.SearchBox);
                Task.Run(() => Dispatcher.Invoke(() => Search.SearchBox.SelectAll()));
            }
        }
    }
}
