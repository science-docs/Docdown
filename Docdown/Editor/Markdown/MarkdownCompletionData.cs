using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using PandocMark.Syntax;
using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace Docdown.Editor.Markdown
{
    public abstract class MarkdownCompletionData : ICompletionData
    {
        public abstract ImageSource Image { get; }

        public abstract string Text { get; }

        public abstract object Content { get; }

        public abstract object Description { get; }

        public double Priority => 0;

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
                    if (text[j] != target[k++])
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

        public static MarkdownFootnoteCompletionData FromReference(Reference reference)
        {
            return new MarkdownFootnoteCompletionData(reference);
        }

        public static IEnumerable<MarkdownFootnoteCompletionData> FromReferences(Block ast)
        {
            var doc = ast.Document;
            foreach (var reference in doc.ReferenceMap.Values)
            {
                yield return FromReference(reference);
            }
        }

        public static IEnumerable<MarkdownLatexCompletionData> LatexData()
        {
            yield break;
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
