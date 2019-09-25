using Docdown.Model;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.IO;
using System.Windows;

namespace Docdown.ViewModel.Commands
{
    public class PrintCommand : DelegateCommand
    {
        public PrintCommand(string fileName, string pdfPath) 
            : base(fileName, pdfPath)
        {
        }

        [Delegate]
        [STAThread]
        private static void Print(string fileName, string pdfPath)
        {
            var dialog = new CommonSaveFileDialog
            {
                Title = Language.Current.Get("Dialog.Print.Title"),
                EnsurePathExists = true,
                DefaultExtension = "pdf",
                CreatePrompt = true,
                DefaultFileName = Path.GetFileNameWithoutExtension(fileName)
            };

            dialog.Filters.Add(new CommonFileDialogFilter("Portable Document Format", ".pdf"));

            if (dialog.ShowDialog(Application.Current.MainWindow) == CommonFileDialogResult.Ok)
            {
                try
                {
                    if (File.Exists(dialog.FileName))
                    {
                        File.Delete(dialog.FileName);
                    }
                    File.Copy(pdfPath, dialog.FileName);
                    AppViewModel.Instance.Messages.Success(Language.Current.Get("Message.File.Save.Success"));
                }
                catch
                {
                    AppViewModel.Instance.Messages.Error(Language.Current.Get("Message.File.Save.Error"));
                }
            }
            dialog.Dispose();
        }
    }
}
