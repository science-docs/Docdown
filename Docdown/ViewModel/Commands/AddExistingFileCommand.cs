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
        [STAThread]
        private async static void OpenAddExistingFileWindow(WorkspaceItemViewModel workspaceItem)
        {
            if (!workspaceItem.IsDirectory)
            {
                throw new ArgumentException("Only directories support the new file command");
            }

            IWorkspaceItem item = workspaceItem.Data;

            using (var dialog = new CommonOpenFileDialog(Language.Current.Get("Dialog.Existing.Title"))
            {
                Multiselect = true,
                IsFolderPicker = false
            })
            {
                if (dialog.ShowDialog(Application.Current.MainWindow) == CommonFileDialogResult.Ok)
                {
                    foreach (var name in dialog.FileNames)
                    {
                        var newChild = await item.CopyExistingItem(name);
                        workspaceItem.AddChild(newChild);
                    }
                }
            }
        }
    }
}
