using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace Docdown.Editor.Markdown
{
    class MarkdownMetaCompletionData : MarkdownCompletionData
    {
        public override ImageSource Image => null;

        public override string Text => metaDataEntry.Name;

        public override TextBlock DescriptionBlock => BuildDescriptionBase("meta", BuildDescription());

        private readonly MetaDataModel.Entry metaDataEntry;

        public MarkdownMetaCompletionData(MetaDataModel.Entry metaDataEntry)
        {
            this.metaDataEntry = metaDataEntry;
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
    }
}
