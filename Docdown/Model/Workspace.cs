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
