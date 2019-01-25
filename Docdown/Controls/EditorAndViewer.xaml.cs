using ICSharpCode.AvalonEdit;
using System.Windows.Controls;

namespace Docdown.Controls
{
    public partial class EditorAndViewer : UserControl, IEditor
    {
        public TextEditor Editor => MdEditor.Editor;

        public EditorAndViewer()
        {
            InitializeComponent();
        }
    }
}