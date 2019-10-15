using Docdown.Model;
using Docdown.Properties;
using Docdown.Util;
using Docdown.ViewModel;
using PandocMark.Syntax;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Docdown.Editor.Markdown
{
    public static class MarkdownValidator
    {
        private static readonly Regex ReferenceRegex = new Regex(@"[^}!]\[([^\]]+)\]", RegexOptions.Compiled);
        private static readonly Regex LocaleRegex = new Regex(@"^[a-z]{2}-[A-Z]{2}$", RegexOptions.Compiled);
        private static readonly Regex AcronymRegex = new Regex(@"\b([A-Z]{2,5})\b", RegexOptions.Compiled);
        private static Regex DiscouragedWords;

        static MarkdownValidator()
        {
            LoadForbiddenWords(Settings.Default.DocumentLocale);
        }

        public static void LoadForbiddenWords(string language)
        {
            string text;
            using (var res = IOUtility.LoadResource($"Docdown.Resources.Validation.Discouraged.{language}.properties"))
            using (var reader = new StreamReader(res))
            {
                text = reader.ReadToEnd();
            }
            var lines = text.Split('\r', '\n');
            var sb = new StringBuilder("(?:[\\s\\.\\[\\]]|^)(");
            sb.Append(string.Join("|", lines.Where(e => !string.IsNullOrEmpty(e)).Select(e => Regex.Escape(e))));
            sb.Append(")(?:[\\s\\.\\[\\]]|$)");

            DiscouragedWords = new Regex(sb.ToString(), RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        public static IEnumerable<Issue> Validate(string text, IWorkspaceItem item)
        {
            return Validate(AbstractSyntaxTree.GenerateAbstractSyntaxTree(text), text, item);
        }

        public static IEnumerable<Issue> Validate(Block document, string text, IWorkspaceItem item)
        {
            foreach (var block in AbstractSyntaxTree.EnumerateBlocks(document))
            {
                foreach (var issue in ValidateBlock(block, text, item))
                {
                    yield return issue;
                }
            }
        }

        private static IEnumerable<Issue> ValidateBlock(Block block, string text, IWorkspaceItem item)
        {
            switch (block.Tag)
            {
                case BlockTag.Paragraph:
                case BlockTag.ReferenceDefinition:
                    foreach (var issue in ValidateText(block, text))
                        yield return issue;

                    break;
                case BlockTag.Meta:
                    foreach (var issue in ValidateMeta(block, item))
                        yield return issue;

                    yield break;
            }
            yield break;
        }

        private static IEnumerable<Issue> ValidateText(Block block, string text)
        {
            int line = TextUtility.GetLineNumber(text, block.SourcePosition) + 1;
            var sub = text.Substring(block.SourcePosition, block.SourceLength);
            var referenceMatches = ReferenceRegex.Matches(sub);
            foreach (var referenceIssue in ValidateRegex(ReferenceRegex, block, sub, line, ReferenceIssueFactory(block)))
            {
                yield return referenceIssue;
            }
            foreach (var discouragedIssue in ValidateRegex(DiscouragedWords, block, sub, line,
                DefaultIssueFactory(block, IssueType.Info, Language.Current.Get("Editor.Discouraged.Word"))))
            {
                yield return discouragedIssue;
            }
            foreach (var acronymIssue in ValidateRegex(AcronymRegex, block, sub, line, AcronymIssueFactory(block)))
            {
                yield return acronymIssue;
            }
        }

        private static Func<Group, string, int, Issue> AcronymIssueFactory(Block block)
        {
            return (group, text, line) =>
            {
                var index = block.SourcePosition + group.Index;
                if (!IsPreceededBy(text, group.Index, "\\ac{", "\\acro{", "^", "@"))
                {
                    var lineNumber = line + TextUtility.GetLineNumber(text, group.Index);
                    var issue = new Issue(IssueType.Info, index, group.Length, lineNumber, "This string is probably an acronym.\nPlease use the appropriate LaTeX Package");
                    return issue;
                }
                return null;
            };
        }

        private static bool IsPreceededBy(string text, int index, params string[] values)
        {
            foreach (var value in values)
            {
                if (index >= value.Length)
                {
                    var before = text.Substring(index - value.Length, value.Length);
                    if (before == value)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static Func<Group, string, int, Issue> DefaultIssueFactory(Block block, IssueType type, string errorText)
        {
            return (group, text, line) =>
            {
                var index = block.SourcePosition + group.Index;
                int lineNumber = line + TextUtility.GetLineNumber(text, group.Index);
                var issue = new Issue(type, index, group.Length, lineNumber, errorText);
                return issue;
            };
        }

        private static Func<Group, string, int, Issue> ReferenceIssueFactory(Block block)
        {
            return (group, text, line) =>
            {
                if (!block.Top.Document.ReferenceMap.ContainsKey(group.Value))
                {
                    var index = block.SourcePosition + group.Index - 1;
                    int lineNumber = line + TextUtility.GetLineNumber(text, group.Index);
                    var issue = new Issue(IssueType.Warning, index, group.Length + 2, lineNumber, 
                        Language.Current.Get("Editor.Reference.Missing"));
                    return issue;
                }
                else
                    return null;
            };
        }

        private static IEnumerable<Issue> ValidateRegex(Regex regex, Block block, string text, int line, Func<Group, string, int, Issue> issueFactory)
        {
            var matches = regex.Matches(text);
            for (int i = 0; i < matches.Count; i++)
            {
                var match = matches[i];
                var group = match.Groups[1];
                if (IsInlined(block, group))
                {
                    continue;
                }
                var issue = issueFactory(group, text, line);
                if (issue != null)
                {
                    yield return issue;
                }
            }
        }

        private static bool IsInlined(Block block, Group group)
        {
            var index = group.Index + block.SourcePosition;
            var inline = AbstractSyntaxTree.SpanningBlockInline(block, index);
            if (inline != null)
            {
                switch (inline.Tag)
                {
                    case InlineTag.Code:
                    case InlineTag.RawHtml:
                        return true;
                }
            }
            return false;
        }

        private static IEnumerable<Issue> ValidateMeta(Block block, IWorkspaceItem item)
        {
            var model = AppViewModel.Instance.Settings.SelectedTemplate.MetaData;

            // When no entries are found, either no meta.json exists for the template
            // or the client cannot connect to the server.
            // Therefore return no validation issues.
            if (model.Entries.Count == 0)
            {
                yield break;
            }

            var set = new HashSet<MetaDataModel.Entry>(model.Entries);
            var existing = new HashSet<string>();

            foreach (var docEntry in block.MetaData.Entries)
            {
                if (!existing.Add(docEntry.Name))
                {
                    yield return new Issue(IssueType.Warning, docEntry.NameStartPosition, docEntry.NameLength, docEntry.LineNumber, "An meta data entry with this name already exists");
                    continue;
                }
                var entry = set.FirstOrDefault(e => e.Name == docEntry.Name);
                if (entry != null)
                {
                    if (!ValidateEntry(entry, docEntry, item, out var defaultMessage))
                    {
                        yield return new Issue(entry.IssueType, docEntry.NameStartPosition, docEntry.NameLength, docEntry.LineNumber, entry.IssueMessage ?? defaultMessage);
                    }
                    set.Remove(entry);
                }
                else
                {
                    yield return new Issue(IssueType.Info, docEntry.NameStartPosition, docEntry.NameLength, docEntry.LineNumber, "This meta data entry is not used by the selected template");
                }
            }

            foreach (var entry in model.Entries)
            {
                if (!entry.IsOptional)
                {
                    yield return new Issue(IssueType.Error, block.SourcePosition, 3, 0, $"Missing necessary entry '{entry.Name}'");
                }
            }
        }

        private static bool ValidateEntry(MetaDataModel.Entry entry, MetaDataEntry doc, IWorkspaceItem item, out string defaultMessage)
        {
            const string DONT_SHOW = "DONT SHOW";
            if (entry.IsArray)
            {
                if (doc.Value == null)
                {
                    foreach (var value in doc.Entries)
                    {
                        if (!ValidateSingleMetaEntry(entry, value.Value, item, out defaultMessage))
                        {
                            return false;
                        }
                    }
                    defaultMessage = DONT_SHOW;
                    return true;
                }
                else
                {
                    defaultMessage = DONT_SHOW;
                    return false;
                }
            }
            else if (doc.Value == null)
            {
                defaultMessage = "Entry has to contain a value";
                return false;
            }
            else
            {
                return ValidateSingleMetaEntry(entry, doc.Value, item, out defaultMessage);
            }
        }

        private static bool ValidateSingleMetaEntry(MetaDataModel.Entry entry, string value, IWorkspaceItem item, out string defaultMessage)
        {
            switch (entry.Type)
            {
                case MetaDataType.None:
                    defaultMessage = $"Entry {value} does not match regex {entry.Regex?.ToString()}";
                    return entry.Regex == null || entry.Regex.IsMatch(value);
                case MetaDataType.Boolean:
                    defaultMessage = "Entry has to be a boolean (true/false)";
                    return bool.TryParse(value, out _);
                case MetaDataType.File:
                    defaultMessage = $"The file '{value}' does not exist in the workspace";
                    return SearchForFile(item.Workspace.Item, value);
                case MetaDataType.Float:
                    defaultMessage = $"Could not parse '{value}' as a floating point number";
                    return double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out _);
                case MetaDataType.Integer:
                    defaultMessage = $"Could not parse '{value}' as an integer";
                    return int.TryParse(value, out _);
                case MetaDataType.Locale:
                    defaultMessage = $"Could not parse '{value}' as a locale (e.g en-US)";
                    return LocaleRegex.IsMatch(value);
            }
            defaultMessage = "";
            return false;
        }

        private static bool SearchForFile(IWorkspaceItem item, string file)
        {
            if (item != item.Workspace.Item)
            {
                var relativeName = item.RelativeName;
                relativeName = relativeName.Substring(relativeName.IndexOf('/') + 1);
                if (file == relativeName)
                {
                    return true;
                }
            }

            foreach (var child in item.Children)
            {
                if (SearchForFile(child, file))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
