using BibTeXLibrary;
using Docdown.Editor;
using Docdown.Editor.BibTex;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;

namespace Docdown.ViewModel.Editing
{
    public class BibEditorViewModel : EditorViewModel
    {
        public override IFoldingStrategy FoldingStrategy => folding;

        private readonly BibFoldingStrategy folding;

        public BibEditorViewModel(WorkspaceItemViewModel item, TextEditor editor) : base(item, editor)
        {
            folding = new BibFoldingStrategy();
        }

        public override void Update()
        {

        }

        public override object FindHoverContent(int index)
        {
            return null;
        }

        public override bool FillCompletionList(CompletionList completionList, int selectionStart)
        {
            return false;
        }

        [ChangeListener(nameof(Text))]
        private void TextChanged()
        {
            folding.Entries.Clear();
            var t = Text;
            if (t != null)
            {
                var entries = BibParser.Parse(t);
                folding.Entries.AddRange(entries);
            }
        }
    }
}
