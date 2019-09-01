using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        public List<Entry> Entries { get; } = new List<Entry>();

        public static MetaDataModel Load(JObject json)
        {
            var meta = new MetaDataModel();
            
            foreach (KeyValuePair<string, JToken> child in json)
            {
                var entry = new Entry
                {
                    Name = child.Key
                };

                var token = child.Value;
                entry.IssueMessage = token.SelectToken("message")?.Value<string>();
                entry.IsOptional = token.SelectToken("optional")?.Value<bool>() ?? false;
                entry.Type = Parse<MetaDataType>(token.SelectToken("type"));
                entry.IssueType = Parse<IssueType>(token.SelectToken("issue"));
                entry.IsArray = token.SelectToken("array")?.Value<bool>() ?? false;
                var regex = token.SelectToken("regex")?.Value<string>();
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

                meta.Entries.Add(entry);
            }

            return meta;
        }

        private static T Parse<T>(JToken token)
        {
            if (token == null || token.Value<string>() == null)
            {
                return default;
            }

            var type = typeof(T);
            if (type.IsEnum)
            {
                var text = token.Value<string>();
                try
                {
                    return (T)Enum.Parse(type, text, true);
                }
                catch
                {
                    return default;
                }
            }
            return default;
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
