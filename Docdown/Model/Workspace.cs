using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
