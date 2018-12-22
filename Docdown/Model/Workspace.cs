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

        public Workspace(string path)
        {
            Item = new WorkspaceItem(path);
        }

        public string Convert()
        {
            if (SelectedItem == null)
                throw new NullReferenceException(nameof(SelectedItem));

            var folder = Path.GetDirectoryName(SelectedItem.FileSystemInfo.FullName);

            var req = WebUtility.MultipartFormDataPost("", "", 
                MultipartFormParameter.FromWorkspaceItem(SelectedItem).ToArray());
            string temp = Path.GetTempFileName();

            using (var rs = req.GetResponseStream())
            {
                using (var fs = File.OpenWrite(temp))
                {
                    rs.CopyTo(fs);
                }
            }
            
            return temp;
        }
    }
}
