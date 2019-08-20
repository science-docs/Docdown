using System.Windows.Controls;
using Docdown.ViewModel;
using ICSharpCode.AvalonEdit;

namespace Docdown.Editor
{
    public partial class DefaultEditor : UserControl, IEditor
    {
        public TextEditor Editor => EditBox;

        private bool firstChange = false;

        public DefaultEditor()
        {
            InitializeComponent();

            Editor.TextChanged += delegate
            {
                if (DataContext is WorkspaceItemViewModel item)
                {
                    if (firstChange)
                    {
                        item.HasChanged = true;
                    }
                }
                firstChange = true;
            };
        }
    }
}
