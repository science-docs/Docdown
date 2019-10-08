using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BibTeXLibrary;
using ICSharpCode.AvalonEdit.Folding;

namespace Docdown.Editor.BibTex
{
    public class BibFoldingStrategy : IFoldingStrategy
    {
        public List<BibEntry> Entries { get; } = new List<BibEntry>();

        public IEnumerable<NewFolding> GenerateFoldings()
        {
            foreach (var entry in Entries)
            {
                yield return new BibFolding(entry);
            }
        }

        private class BibFolding : NewFolding
        {
            public BibFolding(BibEntry entry) : base(entry.SourcePosition, entry.SourcePosition + entry.SourceLength)
            {
                Name = $"@{entry.Type}{{{entry.Key}...";
            }
        }
    }
}
