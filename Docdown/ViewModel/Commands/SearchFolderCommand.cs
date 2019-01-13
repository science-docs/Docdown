using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.IO;
using System.Windows;

namespace Docdown.ViewModel.Commands
{
    public class SearchFolderCommand : DelegateCommand<string>
    {
        public SearchFolderCommand(string initialDirectory, string title, Action<string> callback = null) : 
            base(initialDirectory, title, callback)
        {
        }

        [Delegate]
        private static string OpenFolderDialog(string initialDirectory, string title, Action<string> callback)
        {
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
                Multiselect = false,
                Title = title,
                EnsurePathExists = true
            };

            if (Directory.Exists(initialDirectory))
            {
                dialog.InitialDirectory = initialDirectory;
            }

            if (dialog.ShowDialog(Application.Current.MainWindow) == CommonFileDialogResult.Ok)
            {
                callback?.Invoke(dialog.FileName);
                return dialog.FileName;
            }
            else
            {
                callback?.Invoke(null);
                return null;
            }
        }
    }
}