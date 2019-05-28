using Docdown.Model;
using Docdown.Util;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace Docdown.ViewModel.Commands
{
    public class ImportCommand : DelegateCommand
    {
        public ImportCommand(WorkspaceViewModel workspace, ConverterType target) : base(workspace, target)
        {

        }

        [Delegate]
        private static void Import(WorkspaceViewModel workspace, ConverterType target)
        {
            var dialog = new CommonOpenFileDialog()
            {
                IsFolderPicker = false,
                Multiselect = false,
                EnsureFileExists = true,
                Title = "Select a file to import"
            };

            if (dialog.ShowDialog(Application.Current.MainWindow) == CommonFileDialogResult.Ok)
            {
                try
                {
                    ImportFile(workspace, dialog.FileName, target);
                }
                catch (Exception e)
                {
                    string errorText = ErrorUtility.GetErrorMessage(e);
                    workspace.Messages.Error(errorText);
                }
            }
        }

        private static void ImportFile(WorkspaceViewModel workspace, string fileName, ConverterType target)
        {
            string targetFileName = Path.GetFileNameWithoutExtension(fileName) + target.GetExtension();
            targetFileName = Path.Combine(workspace.Item.RelativeName, targetFileName);
            string from = Path.GetExtension(fileName);
            if (string.IsNullOrEmpty(from))
            {
                throw new InvalidDataException("Could not determine import file type");
            }
            string url = WebUtility.BuildConvertUrl();
            var req = WebUtility.MultipartFormDataPost(url, BuildParameters(fileName, target).Concat(MultipartFormParameter.CreateFile("content", fileName)));
            byte[] content;
            using (var res = req.GetResponse())
            using (var rs = res.GetResponseStream())
            {
                workspace.IgnoreChange = true;
                using (var ms = new MemoryStream())
                {
                    rs.CopyTo(ms);
                    content = ms.ToArray();
                }
                workspace.IgnoreChange = false;
            }
            var item = workspace.Item.Data.CreateNewFile(targetFileName, null, content);
            var itemViewModel = workspace.Item.AddChild(item);
            workspace.SelectedItem = itemViewModel;
        }

        private static IEnumerable<MultipartFormParameter> BuildParameters(string fileName, ConverterType target)
        {
            string fromName = Path.GetExtension(fileName).Substring(1);
            if (Enum.TryParse<ConverterType>(fromName, true, out var from))
            {
                return MultipartFormParameter.ApiParameter(from, target, null, null, true);
            }
            throw new InvalidDataException("Could not determine import file type");
        }
    }
}
