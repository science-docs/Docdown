using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using PandocMark.Syntax;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Docdown.Editor.Markdown
{
    public class MarkdownCitationCompletionData : MarkdownCompletionData
    {
        public override ImageSource Image => Application.Current.TryFindResource("CitationImage") as ImageSource;

        public override string Text => reference.Label.Substring(1);

        public override TextBlock DescriptionBlock => BuildDescriptionBase("citation", reference.Url);

        private readonly Reference reference;

        internal MarkdownCitationCompletionData(Reference reference)
        {
            this.reference = reference;
        }

        public override void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            const string start = "[@";
            const string end = "]";
            var segment = CreateSegment(textArea, completionSegment, Text, start, end);
            textArea.Document.Replace(segment, start + Text + end);
        }
    }
}
