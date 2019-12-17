using System.Collections.Generic;
using System.Linq;
using Docdown.Util;
using System.Xml.Linq;

namespace Docdown.Model
{
    public class WorkspaceItemPersistance
    {
        public string Path { get; set; }
        public int ScrollOffset { get; set; }
        public List<int> ClosedFoldings { get; } = new List<int>();
        public bool IsExpanded { get; set; }
        public bool IsExcluded { get; set; }
        public bool IsSelected { get; set; }

        public bool IsNecessary()
        {
            return ScrollOffset > 0 || ClosedFoldings.Count > 0 || IsExpanded || IsExcluded || IsSelected;
        }

        public XElement ToXml()
        {
            return new XElement(nameof(WorkspaceItemPersistance), 
                new XAttribute(nameof(Path), Path), 
                new XAttribute(nameof(ScrollOffset), ScrollOffset), 
                new XAttribute(nameof(IsExpanded), IsExpanded),
                new XAttribute(nameof(IsSelected), IsSelected),
                new XAttribute(nameof(IsExcluded), IsExcluded),
                ClosedFoldings.Select(e => new XElement(nameof(ClosedFoldings), new XAttribute("Index", e))));
        }

        public static WorkspaceItemPersistance Parse(XElement node)
        {
            var item = new WorkspaceItemPersistance
            {
                Path = node.Attribute<string>(nameof(Path)),
                ScrollOffset = node.Attribute<int>(nameof(ScrollOffset)),
                IsExpanded = node.Attribute<bool>(nameof(IsExpanded)),
                IsSelected = node.Attribute<bool>(nameof(IsSelected)),
                IsExcluded = node.Attribute<bool>(nameof(IsExcluded))
            };

            return item;
        }
    }
}
