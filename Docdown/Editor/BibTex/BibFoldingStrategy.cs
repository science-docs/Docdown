﻿using System.Collections.Generic;
using Docdown.Text.Bib;
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
