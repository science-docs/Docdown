﻿using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.IO;
using System.Windows;

namespace Docdown.ViewModel.Commands
{
    public class PrintCommand : DelegateCommand
    {
        public PrintCommand(string fileName, string pdfPath) : base((Action<string, string>)Print, fileName, pdfPath)
        {
        }

        private static void Print(string fileName, string pdfPath)
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
                File.Copy(pdfPath, dialog.FileName);
            }
        }
    }
}