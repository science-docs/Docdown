using Docdown.Controls;
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

        public override object Content => Text;

        public override TextBlock DescriptionBlock
        {
            get
            {
                var builder = new TextBlockBuilder();
                builder.Image(Image).Text(" citation ", Theme.BlueBrush).Text(Text, Brushes.Aquamarine);
                if (!string.IsNullOrEmpty(reference.Url))
                {
                    builder.LineBreak().Text(reference.Url);
                }
                return builder.Build();
            }
        }

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
