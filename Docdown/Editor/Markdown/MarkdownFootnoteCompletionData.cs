using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using PandocMark.Syntax;
using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Docdown.Editor.Markdown
{
    public class MarkdownFootnoteCompletionData : MarkdownCompletionData
    {
        public override ImageSource Image => Application.Current.TryFindResource("FootnoteImage") as ImageSource;

        public override string Text => reference.Label.Substring(1);

        public override object Content => Text;

        public override object Description
        {
            get
            {
                var span = new Span();
                span.Inlines.Add(new Run("footnote ") { Foreground = Theme.BlueBrush });
                span.Inlines.Add(new Run(Text) { Foreground = Brushes.Aquamarine });
                span.Inlines.Add(new LineBreak());
                span.Inlines.Add(reference.Url);
                return span;
            }
        }

        private readonly Reference reference;

        internal MarkdownFootnoteCompletionData(Reference reference)
        {
            this.reference = reference;
        }

        public override void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            const string start = "[^";
            const string end = "]";
            var segment = CreateSegment(textArea, completionSegment, Text, start, end);
            textArea.Document.Replace(segment, start + Text + end);
        }
    }
}