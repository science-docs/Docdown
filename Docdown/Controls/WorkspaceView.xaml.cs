using Docdown.ViewModel;
using Docdown.ViewModel.Commands;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Docdown.Controls
{
    public partial class WorkspaceView : IWrappedView
    {
        public ICommand CloseCommand => new ActionCommand(Close);

        public WorkspaceView()
        {
            InitializeComponent();
        }

        private void Close()
        {
            AppViewModel.Instance.ExplorerVisible = false;
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

        private async void DropFromTreeView(object sender, DragEventArgs e)
        {
            if (DataContext is Explorer explorer && explorer.Children.Any())
            {
                await HandleDrop(explorer.Children.First(), e);
            }
        }

        private async void DropFromTreeViewItem(object sender, DragEventArgs e)
        {
            if (sender is FrameworkElement elem && elem.DataContext is Explorer explorer)
            {
                if (explorer.WorkspaceItem.IsFile)
                {
                    explorer = explorer.Parent;
                }
                await HandleDrop(explorer, e);
            }
        }

        private async Task HandleDrop(Explorer explorer, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Note that you can have more than one file.
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                // Assuming you have one file that you care about, pass it off to whatever
                // handling code you have defined.
                var workspaceItem = explorer.WorkspaceItem;
                var workspace = workspaceItem.Workspace;
                workspace.IgnoreChange = true;
                var item = workspaceItem.Data;
                try
                {
                    foreach (var name in files)
                    {
                        var isFolder = Directory.Exists(name);
                        var newChild = await (isFolder ? item.CopyExistingFolder(name) : item.CopyExistingItem(name));
                        workspaceItem.AddChild(newChild);
                    }
                }
                catch
                {
                    workspace.Messages.Error("Something went wrong moving files");
                }
                workspace.IgnoreChange = false;
                e.Handled = true;
            }
        }
    }
}