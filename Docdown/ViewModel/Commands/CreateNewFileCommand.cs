using Docdown.Model;
using System;

namespace Docdown.ViewModel.Commands
{
    public class CreateNewFileCommand : DelegateCommand
    {
        public CreateNewFileCommand(WorkspaceItemViewModel workspaceItem) 
            : base(workspaceItem ?? throw new ArgumentNullException(nameof(workspaceItem)))
        {
        }

        [Delegate]
        private static void OpenCreateNewFileWindow(WorkspaceItemViewModel workspaceItem, bool isDirectory = false)
        {
            if (!workspaceItem.IsDirectory)
            {
                throw new ArgumentException("Only directories support the new file command");
            }

            var workspace = workspaceItem.Workspace;
            workspace.IgnoreChange = true;
            IWorkspaceItem newItem;
            if (isDirectory)
            {
                newItem = workspaceItem.Data.CreateNewDirectory("New Folder");
            }
            else
            {
                newItem = workspaceItem.Data.CreateNewFile("NewFile", ".md");
            }
            var vm = new WorkspaceItemViewModel(workspace, workspaceItem, newItem);
            if (isDirectory)
            {
                vm.IsNameChanging = true;
            }
            workspaceItem.AddChild(vm);
            workspace.IgnoreChange = false;
        }
    }
}