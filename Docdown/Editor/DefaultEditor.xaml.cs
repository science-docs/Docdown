using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using Docdown.Util;
using Docdown.ViewModel;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Folding;

namespace Docdown.Editor
{
    public partial class DefaultEditor : UserControl, IEditor
    {
        public TextEditor Editor => EditBox;

        private bool firstChange = false;

        private FoldingManager foldingManager;
        private CompletionWindow completionWindow;
        private readonly ToolTip toolTip = new ToolTip();

        public DefaultEditor()
        {
            InitializeComponent();
            var debounced = ((Action)UpdateDebounced).Debounce(500);

            Editor.TextChanged += delegate
            {
                if (DataContext is WorkspaceItemViewModel item)
                {
                    item.Editor.Text = Editor.Text;
                    if (firstChange)
                    {
                        item.HasChanged = true;
                    }
                    else
                    {
                        SetFirst(item);
                    }
                    var foldingStrategy = item.Editor.FoldingStrategy;
                    if (foldingStrategy != null)
                    {
                        foldingManager.UpdateFoldings(foldingStrategy.GenerateFoldings(), -1);
                    }
                    debounced();
                }
                firstChange = true;
            };

            Editor.TextArea.TextEntered += TextEntered;
            Editor.TextArea.TextEntering += TextEntering;
            Editor.TextArea.PreviewKeyDown += ShowCompletionWindowKeyboard;

            Editor.MouseHover += TextEditorMouseHover;
            Editor.MouseHoverStopped += TextEditorMouseHoverStopped;
        }

        private void UpdateDebounced()
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                if (DataContext is WorkspaceItemViewModel item)
                {
                    item.Editor.Update();
                }
            }));
        }

        void TextEditorMouseHover(object sender, MouseEventArgs e)
        {
            var pos = Editor.GetPositionFromPoint(e.GetPosition(Editor));
            if (pos != null && DataContext is WorkspaceItemViewModel item)
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
            if (e.Text.Length == 1 && DataContext is WorkspaceItemViewModel item)
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
            if (completionWindow == null && DataContext is WorkspaceItemViewModel item)
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

        private void SetFirst(WorkspaceItemViewModel item)
        {
            foreach (var lineTransformer in item.Editor.LineTransformers)
            {
                Editor.TextArea.TextView.LineTransformers.Add(lineTransformer);
            }
            foreach (var backgroundRenderer in item.Editor.BackgroundRenderers)
            {
                Editor.TextArea.TextView.BackgroundRenderers.Add(backgroundRenderer);
            }

            if (foldingManager == null && item.Editor.FoldingStrategy != null)
            {
                foldingManager = FoldingManager.Install(Editor.TextArea);
            }
        }
    }
}
