using ICSharpCode.AvalonEdit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Docdown.Editor.Commands
{
    public static class EditorCommands
    {
        public static EditorCommand Bold { get; } = new BoldCommand();
        public static EditorCommand Italic { get; } = new ItalicCommand();
        public static EditorCommand Quote { get; } = new QuoteCommand();
    }

    public abstract class EditorCommand
    {
        public abstract void Execute(TextEditor editor);

        protected void SorroundParagraphSelection(TextEditor editor, string sorrounding)
        {
            SorroundParagraphSelection(editor, sorrounding, sorrounding);
        }

        protected void SorroundParagraphSelection(TextEditor editor, string sorroundingStart, string sorroundingEnd)
        {
            SorroundParagraph(editor);
            SorroundSelection(editor, sorroundingStart, sorroundingEnd);
        }

        protected void SorroundSelection(TextEditor editor, string sorrounding)
        {
            SorroundSelection(editor, sorrounding, sorrounding);
        }

        protected void SorroundSelection(TextEditor editor, string sorroundingStart, string sorroundingEnd)
        {
            int start = editor.SelectionStart;
            var text = editor.SelectedText;
            var newText = sorroundingStart + text + sorroundingEnd;
            editor.TextArea.Selection.ReplaceSelectionWithText(newText);

            if (string.IsNullOrEmpty(text))
            {
                editor.SelectionStart = start + sorroundingStart.Length;
            }
            else
            {
                editor.SelectionStart = start;
                editor.SelectionLength = newText.Length;
            }
        }

        protected void SorroundParagraph(TextEditor editor)
        {
            int start = editor.SelectionStart;
            int end = start + editor.SelectionLength;
            string text = editor.Text;

            // Decrease start til newline character
            for (; start >= 0 && text[start] != '\n'; start--) ;
            // Increase end til carriage return character
            for (; end < text.Length && text[end] != '\r'; end++) ;

            start++;

            editor.SelectionStart = start;
            editor.SelectionLength = end - start;
        }

        protected void AddText(TextEditor editor, string text)
        {
            int start = editor.SelectionStart;
            string doc = editor.Text;
            int index = doc.Length;
            for (int i = start; i < doc.Length; i++)
            {
                if (doc[i] == '\r')
                {
                    index = i + 2;
                    break;
                }
            }
            doc.Insert(index, text);
            editor.Text = doc;
        }
    }
}
