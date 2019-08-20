using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ICSharpCode.AvalonEdit;

namespace Docdown.Editor.Commands
{
    public abstract class ListCommand : EditorCommand
    {
        private readonly string marker;

        private static readonly HashSet<string> PossibleMarkers = new HashSet<string> { "#.", "•", "*", "+", "-" };

        protected ListCommand(string marker)
        {
            this.marker = marker;
        }

        public override void Execute(TextEditor editor)
        {
            SorroundParagraph(editor);
            InsertMarkerAtParagraph(editor, marker);
        }

        protected void InsertMarkerAtParagraph(TextEditor editor, string marker)
        {
            var paragraphs = Paragraph.Create(editor).ToArray();
            var sb = new StringBuilder();

            for (int i = 0; i < paragraphs.Length; i++)
            {
                var paragraph = paragraphs[i];
                
                if (!string.IsNullOrWhiteSpace(paragraph.Text))
                {
                    paragraph.RemoveListMarker();
                    sb.Append(marker);
                    if (!paragraph.Text.StartsWith(" "))
                    {
                        sb.Append(' ');
                    }
                }
                sb.Append(paragraph.Text);
            }

            editor.SelectedText = sb.ToString();
        }

        private class Paragraph
        {
            public TextEditor Editor { get; set; }
            public string Text { get; set; }

            private static readonly Regex NumberRegex = new Regex(@"^((?:[0-9]+|[#\*\+\•\-a-z])[\.\)])", RegexOptions.Compiled);

            public Paragraph(TextEditor editor, string text)
            {
                Editor = editor;
                Text = text;
            }

            public void RemoveListMarker()
            {
                foreach (var marker in PossibleMarkers)
                {
                    if (Text.StartsWith(marker))
                    {
                        Text = Text.Substring(marker.Length);
                        return;
                    }
                }
                var match = NumberRegex.Match(Text);
                if (match.Groups.Count > 0)
                {
                    Text = Text.Substring(match.Groups[1].Length);
                }
            }

            public static IEnumerable<Paragraph> Create(TextEditor editor)
            {
                var text = editor.Text;
                int i;
                int start = editor.SelectionStart;
                int end = editor.SelectionStart + editor.SelectionLength;

                bool endLine = false;
                for (i = editor.SelectionStart; i < end; i++)
                {
                    char c = text[i];
                    if (endLine)
                    {
                        if (!IsEndLineCharacter(c))
                        {
                            endLine = false;
                            var sub = text.Substring(start, i - start);
                            start = i;
                            yield return new Paragraph(editor, sub);
                        }
                    }
                    else if (IsEndLineCharacter(c))
                    {
                        endLine = true;
                    }
                }

                var rest = text.Substring(start, end - start);
                yield return new Paragraph(editor, rest);
            }

            private static bool IsEndLineCharacter(char c)
            {
                return c == '\r' || c == '\n';
            }

            public static string ToString(IEnumerable<Paragraph> paragraphs)
            {
                return string.Join("", paragraphs); 
            }

            public override string ToString()
            {
                return Text;
            }
        }
    }
}
