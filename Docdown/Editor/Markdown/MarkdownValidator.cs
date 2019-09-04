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
            var sub = text.Substring(block.SourcePosition, block.SourceLength);
            var referenceMatches = ReferenceRegex.Matches(sub);
            foreach (var referenceIssue in ValidateRegex(ReferenceRegex, block, sub, ReferenceIssueFactory(block)))
            {
                yield return referenceIssue;
            }
            foreach (var discouragedIssue in ValidateRegex(DiscouragedWords, block, sub, 
                DefaultIssueFactory(block, IssueType.Info, Language.Current.Get("Editor.Discouraged.Word"))))
            {
                yield return discouragedIssue;
            }
            foreach (var acronymIssue in ValidateRegex(AcronymRegex, block, sub, AcronymIssueFactory(block)))
            {
                yield return acronymIssue;
            }
        }

        private static Func<Group, string, Issue> AcronymIssueFactory(Block block)
        {
            return (group, text) =>
            {
                var index = block.SourcePosition + group.Index;
                var issue = new Issue(IssueType.Info, index, group.Length, "This string is probably an acronym.\nPlease use the appropriate LaTeX Package");
                if (!IsPreceededBy(text, group.Index, "\\ac{", "\\acro{", "^", "@"))
                {
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

        private static Func<Group, string, Issue> DefaultIssueFactory(Block block, IssueType type, string text)
        {
            return (group, _) =>
            {
                var index = block.SourcePosition + group.Index;
                var issue = new Issue(type, index, group.Length)
                {
                    Tooltip = text
                };
                return issue;
            };
        }

        private static Func<Group, string, Issue> ReferenceIssueFactory(Block block)
        {
            return (group, _) =>
            {
                var index = block.SourcePosition + group.Index - 1;
                if (!block.Top.Document.ReferenceMap.ContainsKey(group.Value))
                {
                    var issue = new Issue(IssueType.Warning, index, group.Length + 2)
                    {
                        Tooltip = Language.Current.Get("Editor.Reference.Missing")
                    };
                    return issue;
                }
                else
                    return null;
            };
        }

        private static IEnumerable<Issue> ValidateRegex(Regex regex, Block block, string text, Func<Group, string, Issue> issueFactory)
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
                var issue = issueFactory(group, text);
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
                    yield return new Issue(IssueType.Warning, docEntry.NameStartPosition, docEntry.NameLength, "An meta data entry with this name already exists");
                    continue;
                }
                var entry = set.FirstOrDefault(e => e.Name == docEntry.Name);
                if (entry != null)
                {
                    if (!ValidateEntry(entry, docEntry, item))
                    {
                        yield return new Issue(entry.IssueType, docEntry.NameStartPosition, docEntry.NameLength, entry.IssueMessage);
                    }
                    set.Remove(entry);
                }
                else
                {
                    yield return new Issue(IssueType.Info, docEntry.NameStartPosition, docEntry.NameLength, "This meta data entry is not used by the selected template");
                }
            }

            foreach (var entry in model.Entries)
            {
                if (!entry.IsOptional)
                {
                    yield return new Issue(IssueType.Error, block.SourcePosition, 3, $"Missing necessary entry '{entry.Name}'");
                }
            }
        }

        private static bool ValidateEntry(MetaDataModel.Entry entry, MetaDataEntry doc, IWorkspaceItem item)
        {
            if (entry.IsArray)
            {
                if (doc.Value == null)
                {
                    foreach (var value in doc.Entries)
                    {
                        if (!ValidateSingleMetaEntry(entry, value.Value, item))
                        {
                            return false;
                        }
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (doc.Value == null)
            {
                return false;
            }
            else
            {
                return ValidateSingleMetaEntry(entry, doc.Value, item);
            }
        }

        private static bool ValidateSingleMetaEntry(MetaDataModel.Entry entry, string value, IWorkspaceItem item)
        {
            switch (entry.Type)
            {
                case MetaDataType.None:
                    return entry.Regex == null || entry.Regex.IsMatch(value);
                case MetaDataType.Boolean:
                    return bool.TryParse(value, out _);
                case MetaDataType.File:
                    return SearchForFile(item.Workspace.Item, value);
                case MetaDataType.Float:
                    return double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out _);
                case MetaDataType.Integer:
                    return int.TryParse(value, out _);
                case MetaDataType.Locale:
                    return LocaleRegex.IsMatch(value);
            }
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
