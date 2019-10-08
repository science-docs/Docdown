using Docdown.ViewModel.Editing;
using ICSharpCode.AvalonEdit;

namespace Docdown.Editor
{
    public interface IEditor
    {
        TextEditor Editor { get; }
    }
}