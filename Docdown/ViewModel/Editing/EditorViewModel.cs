using Docdown.Editor;
using Docdown.Util;
using Docdown.ViewModel.Commands;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Collections.Generic;
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

        public IList<IVisualLineTransformer> LineTransformers { get; }
        public IList<IBackgroundRenderer> BackgroundRenderers { get; }

        public abstract IFoldingStrategy FoldingStrategy { get; }

        public ICommand UpdateCommand => new ActionCommand(Update);

        private string text;

        public EditorViewModel(WorkspaceItemViewModel item, TextEditor editor)
        {
            Item = item ?? throw new ArgumentNullException(nameof(item));
            TextEditor = editor ?? throw new ArgumentNullException(nameof(editor));
            LineTransformers = new List<IVisualLineTransformer>();
            BackgroundRenderers = new List<IBackgroundRenderer>();
        }

        public abstract void Update();

        public abstract object FindHoverContent(int index);

        protected Action<int> JumpToLocation()
        {
            return TextEditor.ScrollTo;
        }
    }
}
