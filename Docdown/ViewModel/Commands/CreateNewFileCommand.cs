using System;

namespace Docdown.ViewModel.Commands
{
    public class CreateNewFileCommand : DelegateCommand
    {
        public CreateNewFileCommand(WorkspaceItemViewModel workspaceItem) : 
            base((Action<WorkspaceItemViewModel>)OpenCreateNewFileWindow, workspaceItem)
        {
        }

        private static void OpenCreateNewFileWindow(WorkspaceItemViewModel workspaceItem)
        {

        }
    }
}