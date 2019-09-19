using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace Docdown.ViewModel.Commands
{
    public class SearchFolderCommand : DelegateCommand<string>
    {
        public SearchFolderCommand(string initialDirectory, string title) :
            base(initialDirectory, title, null)
        {
        }

        public SearchFolderCommand(string initialDirectory, string title, Action<string> callback) :
            base(initialDirectory, title, callback)
        {
        }

        public SearchFolderCommand(string initialDirectory, string title, Func<string, Task> callback) :
            base(initialDirectory, title, callback)
        {
        }

        [Delegate]
        [STAThread]
        private static string OpenFolderDialog(string initialDirectory, string title, Delegate callback)
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
                var fileName = dialog.FileName;
                dialog.Dispose();
                callback?.DynamicInvoke(fileName);
                return fileName;
            }
            else
            {
                callback?.DynamicInvoke(new object[] { null });
                dialog.Dispose();
                return null;
            }
        }
    }
}