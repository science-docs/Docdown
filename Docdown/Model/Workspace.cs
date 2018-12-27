using Docdown.Util;
using System;
using System.IO;
using System.Linq;

namespace Docdown.Model
{
    public class Workspace
    {
        public WorkspaceItem Item { get; set; }
        public WorkspaceItem SelectedItem { get; set; }
        public ConverterType FromType => FromSelectedItem();
        public ConverterType ToType { get; set; }
        public string Template { get; set; }

        public Workspace(string path)
        {
            Item = new WorkspaceItem(path);
        }

        public string Convert()
        {
            if (SelectedItem == null)
                throw new NullReferenceException(nameof(SelectedItem));

            var folder = Path.GetDirectoryName(SelectedItem.FileSystemInfo.FullName);
            
            var req = WebUtility.MultipartFormDataPost(WebUtility.BuildConvertUrl(), WebUtility.UserAgent, 
                MultipartFormParameter.ApiParameter(FromType, ToType, Template).Concat(
                MultipartFormParameter.FromWorkspaceItem(SelectedItem)).ToArray());

            string temp = IOUtility.GetTempFile();

            using (var rs = req.GetResponseStream())
            {
                using (var fs = File.OpenWrite(temp))
                {
                    rs.CopyTo(fs);
                }
            }
            
            return temp;
        }

        private ConverterType FromSelectedItem()
        {
            if (SelectedItem == null)
                return ConverterType.Text;

            switch (SelectedItem.Type)
            {
                case WorkspaceItemType.Markdown:
                    return ConverterType.Markdown;
                case WorkspaceItemType.Latex:
                    return ConverterType.Latex;
                default:
                    return ConverterType.Text;
            }
        }

        public override string ToString()
        {
            return Item?.ToString() ?? base.ToString();
        }
    }
}
