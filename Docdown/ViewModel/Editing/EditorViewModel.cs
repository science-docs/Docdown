using Docdown.Editor;
using Docdown.Util;
using Docdown.ViewModel.Commands;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public TextEditor TextEditor { get; }

        public virtual char[] CompletionMarkers { get; }

        public IList<IVisualLineTransformer> LineTransformers { get; }
        public IList<IBackgroundRenderer> BackgroundRenderers { get; }
        public ObservableCollection<MenuItemAction> ContextMenuActions { get; }

        public abstract IFoldingStrategy FoldingStrategy { get; }

        public int ErrorIndex { get; set; } = -1;

        public ICommand UpdateCommand => new ActionCommand(Update);

        private string text;
        private FoldingManager foldingManager;
        private bool firstChange = false;
        private readonly Action debounced;

        public EditorViewModel(WorkspaceItemViewModel item, TextEditor editor)
        {
            Item = item ?? throw new ArgumentNullException(nameof(item));
            TextEditor = editor ?? throw new ArgumentNullException(nameof(editor));
            LineTransformers = new List<IVisualLineTransformer>();
            BackgroundRenderers = new List<IBackgroundRenderer>();
            ContextMenuActions = new ObservableCollection<MenuItemAction>
            {
                new MenuItemAction("Cut", "ImageIcon", ApplicationCommands.Cut),
                new MenuItemAction("Copy", null, ApplicationCommands.Copy),
                new MenuItemAction("Paste", null, ApplicationCommands.Paste)
            };

            editor.TextChanged += EditorTextChanged;
            debounced = UIUtility.Debounce(UpdateInternal, 500);
        }

        private void EditorTextChanged(object sender, EventArgs e)
        {
            Text = TextEditor.Text;
            if (firstChange)
            {
                Item.HasChanged = true;
            }
            else
            {
                InitializeInternal();
            }
            if (FoldingStrategy != null)
            {
                foldingManager.UpdateFoldings(FoldingStrategy.GenerateFoldings(), ErrorIndex);
            }
            debounced();
            firstChange = true;
        }

        private void UpdateInternal()
        {
            Dispatcher.Invoke(Update);
        }

        public abstract void Update();

        public abstract object FindHoverContent(int index);

        public abstract bool FillCompletionList(CompletionList completionList, int selectionStart);

        private void InitializeInternal()
        {
            foldingManager = FoldingManager.Install(TextEditor.TextArea);

            foreach (var lineTransformer in LineTransformers)
            {
                TextEditor.TextArea.TextView.LineTransformers.Add(lineTransformer);
            }
            foreach (var backgroundRenderer in BackgroundRenderers)
            {
                TextEditor.TextArea.TextView.BackgroundRenderers.Add(backgroundRenderer);
            }

            Initialize();
        }

        public virtual void Initialize()
        {

        }

        protected Action<int> JumpToLocation()
        {
            return TextEditor.ScrollTo;
        }
    }
}
