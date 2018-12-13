using Docdown.ViewModel;
using System.Windows;

namespace Docdown.Controls
{
    public partial class WorkspaceView
    {
        public WorkspaceView()
        {
            InitializeComponent();
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is WorkspaceViewModel workspace && 
                e.NewValue is WorkspaceItemViewModel workspaceItem)
            {
                workspace.SelectedItem = workspaceItem;
            }
        }
    }
}