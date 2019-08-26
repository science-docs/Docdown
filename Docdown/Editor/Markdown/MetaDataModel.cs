using Newtonsoft.Json;
using PandocMark.Syntax;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Docdown.Editor.Markdown
{
    public enum MetaDataType
    {
        None = 0,
        String = 0,
        Boolean = 2,
        Integer = 3,
        Float = 1,
        Number = 1,
        Locale = 4,
        File = 5
    }

    public class MetaDataModel
    {
        public static MetaDataModel Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MetaDataModel();
                }
                return instance;
            }
        }

        public List<Entry> Entries { get; } = new List<Entry>();

        private static MetaDataModel instance;

        private MetaDataModel()
        {
        }

        public void Load(string text)
        {
            Entries.Clear();
            var jsonObject = JsonConvert.DeserializeXNode(text, "meta").Root;

            foreach (var child in jsonObject.Elements())
            {
                var entry = new Entry
                {
                    Name = child.Name.LocalName
                };

                entry.IssueMessage = child.Element("message")?.Value;

                var optionalNode = child.Element("optional")?.Value;
                if (optionalNode != null && bool.TryParse(optionalNode, out var optional))
                {
                    entry.IsOptional = optional;
                }
                var typeNode = child.Element("type")?.Value;
                if (typeNode != null && Enum.TryParse<MetaDataType>(typeNode, true, out var type))
                {
                    entry.Type = type;
                }
                var issueType = child.Element("issue")?.Value;
                if (issueType != null && Enum.TryParse<IssueType>(issueType, true, out var issue))
                {
                    entry.IssueType = issue;
                }
                var arrayNode = child.Element("array")?.Value;
                if (arrayNode != null && bool.TryParse(arrayNode, out var isArray))
                {
                    entry.IsArray = isArray;
                }
                var regex = child.Element("regex")?.Value;
                if (regex != null)
                {
                    try
                    {
                        entry.Regex = new Regex(regex);
                    }
                    catch
                    {
                        // Regex was not valid
                    }
                }

                Entries.Add(entry);
            }
        }

        public class Entry
        {
            public string Name { get; set; }
            public bool IsOptional { get; set; }
            public MetaDataType Type { get; set; }
            public IssueType IssueType { get; set; }
            public string IssueMessage { get; set; }
            public bool IsArray { get; set; }
            public Regex Regex { get; set; }
        }
    }
}
