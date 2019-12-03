using Docdown.Editor;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;

namespace Docdown.ViewModel.Editing
{
    public class DefaultEditorViewModel : EditorViewModel
    {
        public override IFoldingStrategy FoldingStrategy => null;

        public DefaultEditorViewModel(WorkspaceItemViewModel item, TextEditor editor) : base(item, editor)
        {
        }

        public override bool FillCompletionList(CompletionList completionList, int selectionStart)
        {
            return false;
        }

        public override object FindHoverContent(int index)
        {
            return null;
        }

        public override void Update()
        {

        }
    }
}
