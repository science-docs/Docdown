using Docdown.ViewModel;
using System.Windows;

namespace Docdown.Controls
{
    public partial class WorkspaceView
    {
        private WorkspaceItemViewModel current;

        public WorkspaceView()
        {
            InitializeComponent();
        }

        private void ViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is WorkspaceViewModel workspace && 
                e.NewValue is WorkspaceItemViewModel workspaceItem)
            {
                current = workspaceItem;
            }
        }

        private void TreeView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (DataContext is WorkspaceViewModel workspace)
            {
                workspace.SelectedItem = current;
            }
        }
    }
}