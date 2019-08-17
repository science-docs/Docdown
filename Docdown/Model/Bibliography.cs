using Docdown.Editor.Markdown;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Docdown.Model
{
    public class BibliographyEntry
    {
        public string Key { get; }
        public IWorkspaceItem Item { get; }
        public string Type { get; }

        public Dictionary<string, string> Fields { get; }

        public BibliographyEntry(IWorkspaceItem item, string key, string type, Dictionary<string, string> fields)
        {
            Item = item;
            Key = key;
            Type = type;
            Fields = fields;
        }
    }

    public class Bibliography
    {
        public IEnumerable<BibliographyEntry> Entries => items.Values.SelectMany(e => e);

        private readonly Dictionary<IWorkspaceItem, BibliographyEntry[]> items = new Dictionary<IWorkspaceItem, BibliographyEntry[]>();

        public void Parse(IWorkspaceItem item, string text = null)
        {
            if (text == null)
            {
                text = File.ReadAllText(item.FullName);
            }
            var entries = BibliographyParser.Parse(item, text);
            items[item] = entries.ToArray();
            var refs = AbstractSyntaxTree.CommonMarkSettings.ExternalReferences;
            refs.Clear();
            refs.AddRange(Entries.Select(e =>
            {
                e.Fields.TryGetValue("title", out string title);
                return new PandocMark.Syntax.Reference("@" + e.Key, title, null);
            }));
        }
    }
}
