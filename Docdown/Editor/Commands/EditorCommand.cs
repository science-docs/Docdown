using ICSharpCode.AvalonEdit;
using System.Text;

namespace Docdown.Editor.Commands
{
    public static class EditorCommands
    {
        public static EditorCommand Bold { get; } = new BoldCommand();
        public static EditorCommand Italic { get; } = new ItalicCommand();
        public static EditorCommand Quote { get; } = new QuoteCommand();
        public static EditorCommand Verbatim { get; } = new VerbatimCommand();
        public static EditorCommand RemoveList { get; } = new RemoveListCommand();
        public static EditorCommand BulletList { get; } = new BulletListCommand();
        public static EditorCommand DotNumberList { get; } = new NumberListCommand(ListFinisher.Dot);
        public static EditorCommand ParenthesisNumberList { get; } = new NumberListCommand(ListFinisher.Parentheses);
        public static EditorCommand DotAlphabeticalList { get; } = new AlphabeticalListCommand(ListFinisher.Dot);
        public static EditorCommand ParenthesisAlphabeticalList { get; } = new AlphabeticalListCommand(ListFinisher.Parentheses);
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
            StringBuilder newText = new StringBuilder(text);
            bool both = false;

            if (!string.IsNullOrEmpty(sorroundingStart) && !text.StartsWith(sorroundingStart))
            {
                newText.Insert(0, sorroundingStart);
            }
            else
            {
                both = true;
            }
            if (!string.IsNullOrEmpty(sorroundingEnd) && !text.EndsWith(sorroundingEnd))
            {
                newText.Append(sorroundingEnd);
                both = false;
            }

            if (both)
            {
                newText.Remove(0, sorroundingStart.Length);
                newText.Remove(newText.Length - sorroundingEnd.Length, sorroundingEnd.Length);
            }

            editor.TextArea.Selection.ReplaceSelectionWithText(newText.ToString());

            editor.SelectionStart = start;
            editor.SelectionLength = newText.Length;
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
