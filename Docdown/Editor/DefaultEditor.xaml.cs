using System.Windows.Controls;
using ICSharpCode.AvalonEdit;

namespace Docdown.Editor
{
    public partial class DefaultEditor : UserControl, IEditor
    {
        public TextEditor Editor => EditBox;

        public DefaultEditor()
        {
            InitializeComponent();
        }
    }
}
