using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace Docdown.Editor.Markdown
{
    public class MarkdownMetaCompletionData : MarkdownCompletionData
    {
        public override ImageSource Image => Application.Current.TryFindResource("TagImage") as ImageSource;

        public override string Text => metaDataEntry.Name;

        public override TextBlock DescriptionBlock => BuildDescriptionBase("meta", BuildDescription());

        private readonly MetaDataModel.Entry metaDataEntry;

        public MarkdownMetaCompletionData(MetaDataModel.Entry metaDataEntry)
        {
            this.metaDataEntry = metaDataEntry ?? throw new ArgumentNullException(nameof(metaDataEntry));
        }

        private string BuildDescription()
        {
            var sb = new StringBuilder();

            switch (metaDataEntry.Type)
            {
                case MetaDataType.String:
                    sb.Append("String ");
                    sb.Append(metaDataEntry.IsArray ? "array" : "value");
                    sb.Append(".");
                    if (metaDataEntry.Regex != null)
                    {
                        sb.Append(" Regex: ");
                        sb.Append(metaDataEntry.Regex.ToString());
                        sb.Append('.');
                    }
                    break;
                case MetaDataType.File:
                    sb.Append("File ");
                    sb.Append(metaDataEntry.IsArray ? "array" : "value");
                    sb.Append('.');
                    break;
            }

            if (metaDataEntry.IsOptional)
            {
                sb.Append(" Is optional");
            }
            else
            {
                sb.Append(" Is necessary for the compilation");
            }

            return sb.ToString();
        }

        public override void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            var sb = new StringBuilder(metaDataEntry.Name);
            sb.Append(": ");
            if (metaDataEntry.IsArray)
            {
                sb.Append("[]");
            }

            var segment = CreateSegment(textArea, completionSegment, Text, null, null);
            textArea.Document.Replace(segment, sb.ToString());
        }

        public override string ToString()
        {
            return Text;
        }
    }
}
