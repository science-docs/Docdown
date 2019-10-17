using Docdown.Model;
using Docdown.Windows;
using System;
using System.Windows;

namespace Docdown.ViewModel.Commands
{
    public class CreateNewFileCommand : DelegateCommand
    {
        public CreateNewFileCommand(WorkspaceItemViewModel workspaceItem) 
            : base(workspaceItem ?? throw new ArgumentNullException(nameof(workspaceItem)))
        {
        }

        [STAThread]
        [Delegate]
        private async static void OpenCreateNewFileWindow(WorkspaceItemViewModel workspaceItem, bool isDirectory = false)
        {
            if (!workspaceItem.IsDirectory)
            {
                throw new ArgumentException("Only directories support the new file command");
            }

            var workspace = workspaceItem.Workspace;
            IWorkspaceItem newItem;
            if (isDirectory)
            {
                newItem = await workspaceItem.Data.CreateNewDirectory(Language.Current.Get("File.New.Folder"));
            }
            else
            {
                var window = new NewFileWindow();
                var viewModel = new NewFileViewModel();
                await viewModel.LoadAsync();
                window.DataContext = viewModel;
                window.Owner = Application.Current.MainWindow;

                if (window.ShowDialog().Value)
                {
                    var sel = viewModel.Selected;
                    newItem = await workspaceItem.Data.CreateNewFile(viewModel.Name, "." + sel.Extension, sel.Content);
                }
                else
                {
                    return;
                }
            }
            var vm = new WorkspaceItemViewModel(workspace, workspaceItem, newItem);
            if (isDirectory)
            {
                vm.IsNameChanging = true;
            }
            workspaceItem.AddChild(vm);
            if (!isDirectory)
            {
                workspaceItem.Workspace.SelectedItem = vm;
            }
        }
    }
}