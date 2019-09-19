using BibTeXLibrary;
using Docdown.Editor.Markdown;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Docdown.Model
{
    public class Bibliography
    {
        public IEnumerable<BibEntry> Entries => items.Values.SelectMany(e => e);

        private readonly Dictionary<IWorkspaceItem, BibEntry[]> items = new Dictionary<IWorkspaceItem, BibEntry[]>();

        public void Parse(IWorkspaceItem item, string text = null)
        {
            if (text == null)
            {
                text = item.FileInfo.FileSystem.File.ReadAllText(item.FullName);
            }
            using (var sr = new StringReader(text))
            {
                try
                {
                    var entries = BibParser.Parse(sr);
                    items[item] = entries.ToArray();
                }
                catch
                {
                    items.Remove(item);
                }
            }
            var refs = AbstractSyntaxTree.CommonMarkSettings.ExternalReferences;
            refs.Clear();
            refs.AddRange(Entries.Select(e =>
            {
                return new PandocMark.Syntax.Reference("@" + e.Key, e.Title, null);
            }));
        }
    }
}
