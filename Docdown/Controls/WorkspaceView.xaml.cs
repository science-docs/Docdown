using Docdown.ViewModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace Docdown.Controls
{
    public partial class WorkspaceView
    {
        public WorkspaceView()
        {
            InitializeComponent();
        }

        private void ViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is Explorer explorer && 
                e.NewValue is Explorer explorerItem)
            {
                explorer.Workspace.PreSelectedItem = explorerItem.WorkspaceItem;
            }
        }

        private void ViewSelectedMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && 
                DataContext is Explorer explorer)
            {
                explorer.Workspace.SelectedItem = explorer.Workspace.PreSelectedItem;
                Keyboard.ClearFocus();
            }
        }
    }
}