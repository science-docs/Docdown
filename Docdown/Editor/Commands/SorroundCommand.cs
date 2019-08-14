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
        protected SorroundCommand(string sorround) : this(sorround, sorround)
        {
            SorroundingStart = sorround;
        }

        protected SorroundCommand(string sorroundStart, string sorroundEnd)
        {
            SorroundingStart = sorroundStart;
            SorroundingEnd = sorroundEnd;
        }

        public override void Execute(TextEditor editor)
        {
            SorroundSelection(editor, SorroundingStart, SorroundingEnd);
        }
    }
}
