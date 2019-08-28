using Docdown.Editor.Markdown;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Docdown.Model
{
    public class BibliographyEntry
    {
        public string Key { get; set; }
        public IWorkspaceItem Item { get; set; }
        public string Type { get; set; }
        public Dictionary<string, string> Fields { get; } = new Dictionary<string, string>();

        public BibliographyEntry()
        {

        }

        public BibliographyEntry(IWorkspaceItem item, string key, string type, Dictionary<string, string> fields)
        {
            Item = item;
            Key = key;
            Type = type;
            Fields = fields;
        }

        public override string ToString()
        {
            var sb = new StringBuilder("@");

            sb.Append(Type);
            sb.Append('{');
            sb.Append(Key);
            if (Fields.Count > 0)
            {
                sb.AppendLine(",");
            }

            foreach (var field in Fields)
            {
                sb.Append(' ');
                sb.Append(field.Key);
                sb.Append(" = {");
                bool useDouble = false;
                if (field.Key == "title")
                {
                    useDouble = true;
                    sb.Append('{');
                }
                sb.Append(field.Value);
                sb.Append('}');
                if (useDouble)
                {
                    sb.Append('}');
                }
                sb.AppendLine(",");
            }

            if (Fields.Count > 0)
            {
                sb.Remove(sb.Length - 3, 3);
                sb.AppendLine();
            }
            sb.Append('}');

            return sb.ToString();
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
