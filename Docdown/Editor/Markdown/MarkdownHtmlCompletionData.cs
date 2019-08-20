using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace Docdown.Editor.Markdown
{
    public class MarkdownHtmlCompletionData : MarkdownCompletionData
    {
        public override ImageSource Image => Application.Current.TryFindResource("HtmlImage") as ImageSource;

        public override string Text => name;

        public override TextBlock DescriptionBlock => BuildDescriptionBase("html", desc);

        private readonly string name;
        private readonly string desc;
        private readonly bool selfClosing;

        private static readonly HashSet<string> SelfClosingTags = new HashSet<string>
        {
            "br", "hr", "img"
        };

        public MarkdownHtmlCompletionData(string name, string description)
        {
            this.name = name;
            selfClosing = SelfClosingTags.Contains(name);
            desc = description;
        }

        public override void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            const string start = "<";
            string end = ">";
            if (name == "!--")
            {
                end = " -->";
            }
            else if (!selfClosing)
            {
                end += $"</{name}>";
            }

            var segment = CreateSegment(textArea, completionSegment, Text, start, end);
            textArea.Document.Replace(segment, start + Text + end);
        }
    }
}
