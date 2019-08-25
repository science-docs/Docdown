using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using ICSharpCode.AvalonEdit;

namespace Docdown.Editor.Commands
{
    public enum ListFinisher
    {
        None, Dot, Parentheses
    }

    public abstract class ListCommand : EditorCommand
    {
        private readonly ListFinisher finisher;
        private readonly Func<int, string> markerSupplier;

        protected ListCommand(string marker, ListFinisher finisher) : this(delegate { return marker ?? string.Empty; }, finisher)
        {

        }

        protected ListCommand(Func<int, string> supplier, ListFinisher finisher)
        {
            this.finisher = finisher;
            markerSupplier = supplier;
        }

        public override void Execute(TextEditor editor)
        {
            SorroundParagraph(editor);
            InsertMarkerAtParagraph(editor, markerSupplier);
        }

        protected void InsertMarkerAtParagraph(TextEditor editor, Func<int, string> markerFactory)
        {
            var paragraphs = Paragraph.Create(editor);
            var sb = new StringBuilder();

            int counter = 0;
            bool onlyWhitespace = true;
            foreach (var paragraph in paragraphs)
            {
                if (!string.IsNullOrWhiteSpace(paragraph.Text))
                {
                    onlyWhitespace = false;
                    paragraph.RemoveListMarker();
                    var marker = markerFactory(counter++);
                    sb.Append(markerFactory(counter++));
                    sb.Append(GetFinisher());
                    if (!string.IsNullOrEmpty(marker))
                    {
                        if (!paragraph.Text.StartsWith(" "))
                        {
                            sb.Append(' ');
                        }
                    }
                    else
                    {
                        paragraph.Text = paragraph.Text.TrimStart();
                    }
                }
                sb.Append(paragraph.Text);
            }
            if (onlyWhitespace)
            {
                var marker = markerFactory(counter);
                if (!string.IsNullOrEmpty(marker))
                {
                    sb.Append(marker);
                    sb.Append(GetFinisher());
                    sb.Append(' ');
                }
            }
            editor.SelectedText = sb.ToString();
        }

        private string GetFinisher()
        {
            switch (finisher)
            {
                case ListFinisher.Dot: return ".";
                case ListFinisher.Parentheses: return ")";
                default: return string.Empty;
            }
        }

        private class Paragraph
        {
            public TextEditor Editor { get; set; }
            public string Text { get; set; }

            private static readonly Regex NumberRegex = new Regex(@"^((?:[0-9]+|[#a-z])[\.\)]|[*\+\•\-])", RegexOptions.Compiled);

            public Paragraph(TextEditor editor, string text)
            {
                Editor = editor;
                Text = text;
            }

            public void RemoveListMarker()
            {
                var match = NumberRegex.Match(Text);
                if (match.Success)
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
