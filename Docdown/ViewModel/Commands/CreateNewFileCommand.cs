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
                newItem = await workspaceItem.Data.CreateNewFile(Language.Current.Get("File.New.File"), ".md");
            }
            var vm = new WorkspaceItemViewModel(workspace, workspaceItem, newItem);
            if (isDirectory)
            {
                vm.IsNameChanging = true;
            }
            workspaceItem.AddChild(vm);
        }
    }
}