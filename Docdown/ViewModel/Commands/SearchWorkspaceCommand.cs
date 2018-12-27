using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.IO;
using System.Windows;

namespace Docdown.ViewModel.Commands
{
    public class SearchWorkspaceCommand : DelegateCommand
    {
        public SearchWorkspaceCommand(string oldWorkspace, Action<string> callback) : 
            base((Action<string, Action<string>>)OpenFolderDialog, oldWorkspace, callback)
        {
        }

        private static void OpenFolderDialog(string oldWorkspace, Action<string> callback)
        {
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
                Multiselect = false,
                Title = "Select workspace",
                EnsurePathExists = true
            };

            if (Directory.Exists(oldWorkspace))
            {
                dialog.InitialDirectory = oldWorkspace;
            }

            if (dialog.ShowDialog(Application.Current.MainWindow) == CommonFileDialogResult.Ok)
            {
                callback?.Invoke(dialog.FileName);
            }
            else
            {
                callback?.Invoke(null);
            }
        }
    }
}