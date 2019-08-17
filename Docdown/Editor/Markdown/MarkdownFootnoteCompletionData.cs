using Docdown.Controls;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using PandocMark.Syntax;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Docdown.Editor.Markdown
{
    public class MarkdownFootnoteCompletionData : MarkdownCompletionData
    {
        public override ImageSource Image => Application.Current.TryFindResource("FootnoteImage") as ImageSource;

        public override string Text => reference.Label.Substring(1);

        public override object Content => Text;

        public override TextBlock DescriptionBlock =>
            new TextBlockBuilder()
                .Image(Image)
                .Text(" footnote ", Theme.BlueBrush)
                .Text(Text, Brushes.Aquamarine)
                .LineBreak()
                .Text(reference.Url)
                .Build();

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