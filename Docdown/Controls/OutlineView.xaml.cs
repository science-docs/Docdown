using Docdown.ViewModel;
using System.Windows;
using System.Windows.Input;

namespace Docdown.Controls
{
    public partial class OutlineView : IWrappedView
    {
        public ICommand CloseCommand => TreeGrid.RemoveTreeGridItemCommand(Parent);

        public OutlineView()
        {
            InitializeComponent();
        }

        private void OutlineSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is AppViewModel app && 
                e.NewValue is OutlineItemViewModel outlineItem)
            {
                if (app.Workspace.SelectedItem != null)
                {
                    app.Workspace.SelectedItem.SelectedOutlineItem = outlineItem;
                }
            }
        }

        private void OutlineMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && 
                DataContext is AppViewModel app)
            {
                var selectedItem = app.Workspace.SelectedItem?.SelectedOutlineItem;
                var outline = app.Workspace.SelectedItem?.Outline;
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
