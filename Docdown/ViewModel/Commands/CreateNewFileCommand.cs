using System;

namespace Docdown.ViewModel.Commands
{
    public class CreateNewFileCommand : DelegateCommand
    {
        public CreateNewFileCommand(WorkspaceItemViewModel workspaceItem) : base(workspaceItem)
        {
        }

        [Delegate]
        private static void OpenCreateNewFileWindow(WorkspaceItemViewModel workspaceItem)
        {

        }
    }
}