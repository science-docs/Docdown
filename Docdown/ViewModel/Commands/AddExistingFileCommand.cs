using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Docdown.ViewModel.Commands
{
    public class AddExistingFileCommand : DelegateCommand
    {
        public AddExistingFileCommand(WorkspaceItemViewModel workspaceItem)
            : base(workspaceItem ?? throw new ArgumentNullException(nameof(workspaceItem)))
        {
        }

        [Delegate]
        private static void OpenAddFileWindow(WorkspaceItemViewModel workspaceItem)
        {
            if (!workspaceItem.IsDirectory)
            {
                throw new ArgumentException("Only directories support the new file command");
            }

            var workspace = workspaceItem.Workspace;
            workspace.IgnoreChange = true;
            var fullPath = workspaceItem.FullName;
            WorkspaceItem newItem;
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
