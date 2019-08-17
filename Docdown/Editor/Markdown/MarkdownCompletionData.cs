using Docdown.Model;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using PandocMark.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Docdown.Editor.Markdown
{
    public abstract class MarkdownCompletionData : ICompletionData
    {
        public const char LatexMarker = '\\';
        public const char CitationMarker = '@';
        public const char FootnoteMarker = '^';

        public abstract ImageSource Image { get; }

        public abstract string Text { get; }

        public abstract object Content { get; }

        public object Description => DescriptionBlock;

        public abstract TextBlock DescriptionBlock { get; }

        public virtual double Priority => 0;

        public virtual void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, Text);
        }

        public ISegment CreateSegment(TextArea textArea, ISegment current, string target, string supposedStart, string supposedEnd)
        {
            var text = textArea.Document.Text;
            int start = current.Offset;
            int end = current.EndOffset;

            if (!string.IsNullOrEmpty(target))
            {
                start = ChangeStartByTarget(start, text, target);
                end = ChangeEndByTarget(end, text, target);
            }


            if (!string.IsNullOrEmpty(supposedStart))
            {
                for (int i = 1; i <= supposedStart.Length && start > 0; i++)
                {
                    int index = supposedStart.Length - i;
                    if (text[start - 1] == supposedStart[index])
                    {
                        start--;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (!string.IsNullOrEmpty(supposedEnd))
            {
                for (int i = 1; i <= supposedEnd.Length && end < text.Length; i++)
                {
                    int index = i - 1;
                    if (text[end] == supposedEnd[index])
                    {
                        end++;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return new Segment(start, end);
        }

        private int ChangeStartByTarget(int start, string text, string target)
        {
            for (int i = target.Length; i > 0 && start - i >= 0; i--)
            {
                bool contains = true;
                int subStart = start - i;
                int k = 0;
                for (int j = subStart; j < start; j++)
                {
                    if (NotEqual(text[j], target[k++]))
                    {
                        contains = false;
                        break;
                    }
                }
                if (contains)
                {
                    return subStart;
                }
            }
            return start;
        }

        private int ChangeEndByTarget(int end, string text, string target)
        {
            for (int i = 0; i < target.Length && end + i < text.Length; i++)
            {
                bool contains = true;
                int subEnd = end + i;
                int k = target.Length - 1;
                for (int j = subEnd; j >= end; j--)
                {
                    if (NotEqual(text[j], target[k--]))
                    {
                        contains = false;
                        break;
                    }
                }
                if (contains)
                {
                    return subEnd + 1;
                }
            }
            return end;
        }

        private bool NotEqual(char a, char b)
        {
            return char.ToLowerInvariant(a) != char.ToLowerInvariant(b);
        }

        public static void FromAST(string text, int index, Block ast, CompletionList list)
        {
            char? marker = IdentifyMarker(text, index, out string pre);
            var data = new List<ICompletionData>();
            if (marker == null || marker == FootnoteMarker)
            {
                data.AddRange(FromReferences(ast));
            }
            if (marker == null || marker == CitationMarker)
            {
                data.AddRange(FromBibliography(ast));
            }
            if (marker == null || marker == LatexMarker)
            {
                data.AddRange(LatexData());
            }
            data.Sort((a, b) => a.Text.CompareTo(b.Text));
            foreach (var item in data)
            {
                list.CompletionData.Add(item);
            }
            if (!string.IsNullOrWhiteSpace(pre))
            {
                list.IsFiltering = true;
                list.SelectItem(pre);
            }
        }

        private static char? IdentifyMarker(string text, int index, out string pretext)
        {
            for (int i = index - 1; i >= 0; i--)
            {
                char c = text[i];
                if (IsEndCharacter(c))
                {
                    pretext = Sub(i);
                    return null;
                }
                if (IsMarker(c))
                {
                    pretext = Sub(i);
                    return c;
                }
            }
            pretext = text.Substring(0, index);
            return null;

            string Sub(int i)
            {
                return text.Substring(i + 1, index - i - 1);
            }
        }

        private static bool IsEndCharacter(char c)
        {
            return c == '[' || c == ']' || c == ' ' || c == '\n';
        }

        private static bool IsMarker(char c)
        {
            return c == LatexMarker || c == CitationMarker || c == FootnoteMarker;
        }

        public static MarkdownFootnoteCompletionData FromReference(Reference reference)
        {
            return new MarkdownFootnoteCompletionData(reference);
        }

        public static IEnumerable<MarkdownFootnoteCompletionData> FromReferences(Block ast)
        {
            var doc = ast.Document;
            foreach (var reference in doc.ReferenceMap.Values.Where(e => StartsWith(e.Label, FootnoteMarker)))
            {
                yield return FromReference(reference);
            }
        }

        public static IEnumerable<MarkdownLatexCompletionData> LatexData()
        {
            yield break;
        }

        public static IEnumerable<MarkdownCitationCompletionData> FromBibliography(Block ast)
        {
            foreach (var entry in ast.Document.ReferenceMap.Values.Where(e => StartsWith(e.Label, CitationMarker)))
            {
                yield return new MarkdownCitationCompletionData(entry);
            }
        }

        private static bool StartsWith(string value, char c)
        {
            return value != null && value.Length > 0 && value[0] == c;
        }

        private struct Segment : ISegment
        {
            public int Offset => Off;

            public int Length => End - Off;

            public int EndOffset => End;

            public int Off;
            public int End;

            public Segment(int start, int end)
            {
                Off = start;
                End = end;
            }
        }
    }
}
