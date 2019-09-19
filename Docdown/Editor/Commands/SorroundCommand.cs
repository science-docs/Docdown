using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.AvalonEdit;

namespace Docdown.Editor.Commands
{
    public abstract class SorroundCommand : EditorCommand
    {
        public string SorroundingStart { get; }
        public string SorroundingEnd { get; }

        private bool sorroundParagraph;

        protected SorroundCommand(string sorround) : this(sorround, sorround)
        {
            SorroundingStart = sorround;
        }

        protected SorroundCommand(string sorroundStart, string sorroundEnd)
        {
            SorroundingStart = sorroundStart;
            SorroundingEnd = sorroundEnd;
        }

        protected SorroundCommand(string sorroundStart, string sorroundEnd, bool sorroundParagraph)
        {
            SorroundingStart = sorroundStart;
            SorroundingEnd = sorroundEnd;
            this.sorroundParagraph = sorroundParagraph;
        }

        public override void Execute(TextEditor editor)
        {
            if (sorroundParagraph)
            {
                SorroundParagraph(editor);
            }
            SorroundSelection(editor, SorroundingStart, SorroundingEnd);
        }
    }
}
