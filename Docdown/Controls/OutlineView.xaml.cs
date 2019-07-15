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

        private void OutlineSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
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

        private void OutlineMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && 
                DataContext is WorkspaceViewModel workspace)
            {
                var selectedItem = workspace?.SelectedItem?.SelectedOutlineItem;
                var outline = workspace?.SelectedItem?.Outline;
                if (outline?.JumpTo != null && selectedItem != null)
                {
                    Keyboard.ClearFocus();
                    outline.JumpTo(selectedItem.Data.JumpPosition);
                    e.Handled = true;
                }
            }
        }
    }
}
