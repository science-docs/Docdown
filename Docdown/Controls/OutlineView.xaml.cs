using Docdown.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Docdown.Controls
{
    public partial class OutlineView : UserControl
    {
        public OutlineView()
        {
            InitializeComponent();
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is WorkspaceViewModel workspace && 
                e.NewValue is OutlineItemViewModel outlineItem)
            {
                if (workspace.SelectedItem != null)
                {
                    workspace.SelectedItem.SelectedOutlineItem = outlineItem;
                }
            }
        }

        private void TreeView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is WorkspaceViewModel workspace)
            {
                var selectedItem = workspace?.SelectedItem?.SelectedOutlineItem;
                var outline = workspace?.SelectedItem?.Outline;
                if (outline != null && selectedItem != null)
                {
                    outline?.JumpTo(selectedItem.Data.TextPosition);
                    e.Handled = true;
                }
            }
        }
    }
}
