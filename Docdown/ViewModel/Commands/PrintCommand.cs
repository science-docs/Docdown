using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.IO;
using System.Windows;

namespace Docdown.ViewModel.Commands
{
    public class PrintCommand : DelegateCommand
    {
        public PrintCommand(WorkspaceViewModel workspace, string fileName, string pdfPath) 
            : base(workspace ?? throw new ArgumentNullException(nameof(workspace)), fileName, pdfPath)
        {
        }

        [Delegate]
        private static void Print(WorkspaceViewModel workspace, string fileName, string pdfPath)
        {
            var dialog = new CommonSaveFileDialog
            {
                Title = "Save Pdf file",
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
                        File.Replace(pdfPath, dialog.FileName, null);
                    }
                    else
                    {
                        File.Copy(pdfPath, dialog.FileName);
                    }
                }
                catch
                {
                    workspace.Messages.Error("Could not save file", "");
                }
            }
        }
    }
}
