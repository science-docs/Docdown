using Docdown.Model;
using Docdown.Util;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Docdown.ViewModel.Commands
{
    public class ImportCommand : DelegateCommand
    {
        public ImportCommand(WorkspaceViewModel workspace, string target) : base(workspace, target)
        {

        }

        [Delegate]
        private static void Import(WorkspaceViewModel workspace, string target)
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
                string fileName = dialog.FileName;
                string targetFileName = Path.GetFileNameWithoutExtension(fileName) + ".md";
                targetFileName = Path.Combine(workspace.Item.FullName, targetFileName);
                string from = Path.GetExtension(fileName).Substring(1);
                string url = WebUtility.BuildConvertUrl();
                var req = WebUtility.MultipartFormDataPost(url, BuildParameters(fileName, target).Concat(MultipartFormParameter.CreateFile("content", fileName)));
                using (var res = req.GetResponse())
                using (var rs = res.GetResponseStream())
                {
                    workspace.IgnoreChange = true;
                    using (var fs = File.Open(targetFileName, FileMode.OpenOrCreate))
                    {
                        rs.CopyTo(fs);
                    }
                    workspace.IgnoreChange = false;
                }
                var item = new WorkspaceItem(new FileInfo(targetFileName));
                workspace.Item.AddChild(item);
            }
        }

        private static IEnumerable<MultipartFormParameter> BuildParameters(string fileName, string target)
        {
            string fromName = Path.GetExtension(fileName).Substring(1);
            if (Enum.TryParse<ConverterType>(target, true, out var to) &&
                Enum.TryParse<ConverterType>(fromName, true, out var from))
            {
                return MultipartFormParameter.ApiParameter(from, to, null, null);
            }
            throw new InvalidDataException("Could not determine import file type");
        }
    }
}
