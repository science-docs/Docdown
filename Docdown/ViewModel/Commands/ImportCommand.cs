using Docdown.Model;
using Docdown.Util;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Docdown.ViewModel.Commands
{
    public class ImportCommand : DelegateCommand
    {
        public ImportCommand(WorkspaceViewModel workspace, ConverterType target) : base(workspace, target)
        {

        }

        [Delegate]
        [STAThread]
        private async static void Import(WorkspaceViewModel workspace, ConverterType target)
        {
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = false,
                Multiselect = false,
                EnsureFileExists = true,
                Title = Language.Current.Get("Dialog.Import.Title")
            };

            if (dialog.ShowDialog(Application.Current.MainWindow) == CommonFileDialogResult.Ok)
            {
                try
                {
                    await ImportFile(workspace, dialog.FileName, target);
                }
                catch (Exception e)
                {
                    var errorText = await ErrorUtility.GetErrorMessage(e, workspace.Dispatcher);
                    workspace.Messages.Error(errorText);
                }
            }
            dialog.Dispose();
        }

        private async static Task ImportFile(WorkspaceViewModel workspace, string fileName, ConverterType target)
        {
            string targetFileName = Path.GetFileNameWithoutExtension(fileName) + target.GetExtension();
            targetFileName = Path.Combine(workspace.Item.RelativeName, targetFileName);
            string from = Path.GetExtension(fileName);
            if (string.IsNullOrEmpty(from))
            {
                throw new InvalidDataException("Could not determine import file type");
            }
            string url = WebUtility.BuildConvertUrl();
            using (var req = await WebUtility.PostRequest(url, BuildParameters(fileName, target).Concat(MultipartFormParameter.CreateFile("content", fileName))))
            {
                byte[] content = await req.Content.ReadAsByteArrayAsync();
                var item = await workspace.Item.Data.CreateNewFile(targetFileName, null, content);
                var itemViewModel = workspace.Item.AddChild(item);
                workspace.SelectedItem = itemViewModel;
            }
        }

        private static IEnumerable<MultipartFormParameter> BuildParameters(string fileName, ConverterType target)
        {
            string fromName = Path.GetExtension(fileName).Substring(1);
            if (Enum.TryParse<ConverterType>(fromName, true, out var from))
            {
                return MultipartFormParameter.ApiParameter(from, target, null, null);
            }
            throw new InvalidDataException("Could not determine import file type");
        }
    }
}
