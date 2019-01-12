using Docdown.ViewModel;
using System.Windows;
using System.Windows.Input;

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

        private void ViewSelectedMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && 
                DataContext is WorkspaceViewModel workspace)
            {
                workspace.SelectedItem = current;
                Keyboard.ClearFocus();
            }
        }
    }
}