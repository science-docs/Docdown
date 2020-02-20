using Docdown.Editor;
using Docdown.Util;
using Docdown.ViewModel.Commands;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Folding;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;

namespace Docdown.ViewModel.Editing
{
    public abstract class EditorViewModel : ObservableObject
    {
        public string Text
        {
            get => text;
            set
            {
                if (text != null)
                {
                    Item.HasChanged = true;
                }
                Set(ref text, value);
            }
        }

        public WorkspaceItemViewModel Item { get; }

        public virtual char[] CompletionMarkers { get; }

        public ObservableCollection<MenuItemAction> ContextMenuActions { get; }

        public abstract IFoldingStrategy FoldingStrategy { get; }

        public int ErrorIndex { get; set; } = -1;

        public ICommand UpdateCommand => new ActionCommand(Update);

        public Action<int> JumpTo { get; private set; }

        private string text;
        private FoldingManager foldingManager;
        private Action<TextEditor> debounced;

        private readonly ToolTip toolTip = new ToolTip();
        private CompletionWindow completionWindow;

        protected EditorViewModel(WorkspaceItemViewModel item)
        {
            Item = item ?? throw new ArgumentNullException(nameof(item));
            ContextMenuActions = new ObservableCollection<MenuItemAction>
            {
                new MenuItemAction("Cut", null, ApplicationCommands.Cut),
                new MenuItemAction("Copy", null, ApplicationCommands.Copy),
                new MenuItemAction("Paste", null, ApplicationCommands.Paste)
            };
            
        }

        public virtual void Configure(TextEditor editor)
        {
            debounced = UIUtility.Debounce<TextEditor>(UpdateInternal, 500);

            JumpTo = JumpToLocation(editor);

            Text = editor.Text = Encoding.UTF8.GetString(Item.Data.Read());

            foldingManager = FoldingManager.Install(editor.TextArea);

            if (FoldingStrategy != null)
            {
                foldingManager.UpdateFoldings(FoldingStrategy.GenerateFoldings(), ErrorIndex);
            }

            editor.TextArea.TextEntered += TextEntered;
            editor.TextArea.TextEntering += TextEntering;
            editor.TextArea.PreviewKeyDown += ShowCompletionWindowKeyboard;

            editor.MouseHover += MouseHover;
            editor.MouseHoverStopped += MouseHoverStopped;
            editor.TextChanged += EditorTextChanged;

            debounced(editor);
        }

        private void MouseHover(object sender, MouseEventArgs e)
        {
            var editor = (TextEditor)sender;
            var pos = editor.GetPositionFromPoint(e.GetPosition(editor));
            if (pos != null)
            {
                int index = editor.Document.GetOffset(pos.Value.Line, pos.Value.Column);
                var content = FindHoverContent(index);
                if (content != null)
                {
                    toolTip.PlacementTarget = editor;
                    toolTip.Content = content;
                    toolTip.IsOpen = true;
                    e.Handled = true;
                }
            }
        }

        private void MouseHoverStopped(object sender, MouseEventArgs e)
        {
            toolTip.IsOpen = false;
        }

        private void EditorTextChanged(object sender, EventArgs e)
        {
            var editor = (TextEditor)sender;

            Text = editor.Text;
            Item.HasChanged = true;
            if (FoldingStrategy != null)
            {
                foldingManager.UpdateFoldings(FoldingStrategy.GenerateFoldings(), ErrorIndex);
            }
            debounced(editor);
        }

        private void ShowCompletionWindow(TextArea area)
        {
            if (completionWindow == null)
            {
                completionWindow = new CompletionWindow(area);

                int startPosition = 0;
                if (area.Selection.IsEmpty)
                    startPosition = area.Caret.Offset;
                else
                    startPosition = area.Selection.SurroundingSegment.Offset;

                if (FillCompletionList(completionWindow.CompletionList, startPosition))
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

        private void ShowCompletionWindowKeyboard(object sender, KeyEventArgs e)
        {
            if (sender is TextArea area && completionWindow == null && e.Key == Key.Space && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                ShowCompletionWindow(area);
                e.Handled = true;
            }
        }

        private void TextEntered(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length == 1 && sender is TextArea textArea)
            {
                var c = e.Text[0];
                if (CompletionMarkers != null && CompletionMarkers.Contains(c))
                {
                    ShowCompletionWindow(textArea);
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

        private void UpdateInternal(TextEditor editor)
        {
            Dispatcher.Invoke(() =>
            {
                editor.TextArea.TextView.Redraw();
                Update();
            });
        }

        public abstract void Update();

        public abstract object FindHoverContent(int index);

        public abstract bool FillCompletionList(CompletionList completionList, int selectionStart);

        protected static Action<int> JumpToLocation(TextEditor editor)
        {
            return editor.ScrollTo;
        }
    }
}
