using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.IO;
using System.Windows;

namespace Docdown.ViewModel.Commands
{
    public class SearchFileCommand : DelegateCommand<string>
    {
        public SearchFileCommand(string initialDirectory, string title, Action<string> callback, params CommonFileDialogFilter[] filters) :
            base(initialDirectory, title, callback, filters)
        {
        }

        [Delegate]
        private static string OpenFileDialog(string initialDirectory, string title, Action<string> callback, params CommonFileDialogFilter[] filters)
        {
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = false,
                Multiselect = false,
                Title = title,
                EnsurePathExists = true
            };

            if (filters != null)
            {
                foreach (var filter in filters)
                {
                    dialog.Filters.Add(filter);
                }
            }

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
