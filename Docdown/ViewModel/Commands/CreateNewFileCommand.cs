using Docdown.Model;
using System;
using System.IO;

namespace Docdown.ViewModel.Commands
{
    public class CreateNewFileCommand : DelegateCommand
    {
        public CreateNewFileCommand(WorkspaceItemViewModel workspaceItem) : base(workspaceItem)
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

            var fullPath = workspaceItem.FullName;
            FileSystemInfo fsi;
            if (isDirectory)
            {
                var newDir = Path.Combine(fullPath, "New Folder");
                Directory.CreateDirectory(newDir);
                fsi = new DirectoryInfo(newDir);
            }
            else
            {
                var newFile = Path.Combine(fullPath, "NewFile.md");
                File.Create(newFile).Close();
                fsi = new FileInfo(newFile);
            }
            var item = new WorkspaceItem(fsi, false);
            var vm = new WorkspaceItemViewModel(workspace, workspaceItem, item);
            if (isDirectory)
            {
                vm.IsNameChanging = true;
            }
            workspaceItem.AddChild(vm);
        }
    }
}