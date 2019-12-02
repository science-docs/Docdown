using Docdown.Model;
using Docdown.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Docdown.Text.Markdown
{
    public class MarkdownPreprocessor : Preprocessor
    {
        public override string Preprocess(IWorkspaceItem item, string content)
        {
            content = ProcessTables(item, content);
            return content;
        }

        private static readonly Regex TableRegex = new Regex(@"!\[(.*)\]\((.*\.csv)\)", RegexOptions.Compiled);

        private string ProcessTables(IWorkspaceItem item, string content)
        {
            var itemName = item.FullName;
            var parentName = Path.GetDirectoryName(itemName);
            var matches = TableRegex.Matches(content);
            int addIndex = 0;
            foreach (Match match in matches)
            {
                var caption = match.Groups[1];
                var file = match.Groups[2];
                
                var fileName = Path.Combine(parentName, file.Value);

                var fileItem = item.Workspace.FindByName(fileName);
                string table;
                if (fileItem != null)
                {
                    try
                    {

                        var text = Encoding.UTF8.GetString(fileItem.Read());
                        table = CsvToMarkdown(text, caption.Value);
                    }
                    catch
                    {
                        table = "**Could not parse table file**";
                    }
                }
                else
                {
                    table = $"**Could not find {file}**";
                }
                var indexDelta = table.Length - match.Length;
                
                content = content.Replace(addIndex + match.Index, match.Length, table);

                addIndex += indexDelta;
            }

            return content;
        }

        private string CsvToMarkdown(string csv, string caption)
        {
            var lines = csv.Split('\n');
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            var headers = lines[0].Split(';').Select(e => e.Trim()).ToArray();
            sb.Append('|');
            sb.Append(string.Join("|", headers.Select(e => e.Trim(':')).Where(e => !string.IsNullOrWhiteSpace(e))));
            sb.AppendLine("|");
            sb.Append('|');
            for (int i = 0; i < headers.Length; i++)
            {
                var header = headers[i];
                if (header.StartsWith(":"))
                {
                    sb.Append(':');
                }
                sb.Append("---");
                if (header.EndsWith(":"))
                {
                    sb.Append(':');
                }
                sb.Append('|');
            }
            sb.AppendLine();
            for (int i = 1; i < lines.Length; i++)
            {
                var items = lines[i].Split(';');
                sb.Append('|');
                sb.Append(string.Join("|", items.Select(e => e.Trim()).Where(e => !string.IsNullOrWhiteSpace(e))));
                sb.AppendLine("|");
            }

            sb.AppendLine();
            sb.Append(": ");
            sb.AppendLine(caption);
            sb.AppendLine();

            return sb.ToString();
        }
    }
}
