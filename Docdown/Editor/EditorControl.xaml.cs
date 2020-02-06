using System.Windows.Controls;
using Docdown.Util;
using Docdown.ViewModel;
using ICSharpCode.AvalonEdit;

namespace Docdown.Editor
{
    public partial class EditorControl : UserControl, IEditor
    {
        public TextEditor Editor => EditBox;

        public EditorControl()
        {
            InitializeComponent();

            Editor.Options.IndentationSize = 4;
            Editor.Options.ConvertTabsToSpaces = true;
            Editor.Options.AllowScrollBelowDocument = true;
            Editor.Options.EnableHyperlinks = false;
            Editor.Options.EnableEmailHyperlinks = false;
            Editor.Options.EnableRectangularSelection = false;
            Editor.Options.HighlightCurrentLine = true;

            DataContextChanged += ViewModelChanged;
        }

        private void ViewModelChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is WorkspaceItemViewModel viewModel)
            {
                viewModel.Editor.Configure(Editor);
                Editor.TextArea.TextView.Redraw();
            }
        }

        private void SearchControl_IsVisibleChanged(object _1, System.Windows.DependencyPropertyChangedEventArgs _2)
        {
            if (Search.IsVisible)
            {
                UIUtility.Focus(this, Search.SearchBox);
            }
        }

        private void EditBox_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (Editor.SelectionLength == 0)
            {
                var index = Editor.MouseIndex();
                if (index >= 0)
                {
                    Editor.SelectionStart = index;
                }
            }
        }
    }
}
