using Docdown.Model;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Windows;

namespace Docdown.ViewModel.Commands
{
    public class AddExistingFileCommand : DelegateCommand
    {
        public AddExistingFileCommand(WorkspaceItemViewModel workspaceItem)
            : base(workspaceItem ?? throw new ArgumentNullException(nameof(workspaceItem)))
        {
        }

        [Delegate]
        private static void OpenAddExistingFileWindow(WorkspaceItemViewModel workspaceItem)
        {
            if (!workspaceItem.IsDirectory)
            {
                throw new ArgumentException("Only directories support the new file command");
            }

            var workspace = workspaceItem.Workspace;
            workspace.IgnoreChange = true;
            WorkspaceItem item = workspaceItem.Data;

            var dialog = new CommonOpenFileDialog("Add existing files")
            {
                Multiselect = true,
                IsFolderPicker = false
            };

            if (dialog.ShowDialog(Application.Current.MainWindow) == CommonFileDialogResult.Ok)
            {
                foreach (var name in dialog.FileNames)
                {
                    var newChild = item.CopyExistingItem(name);
                    workspaceItem.AddChild(newChild);
                }
            }
            workspace.IgnoreChange = false;
        }
    }
}
