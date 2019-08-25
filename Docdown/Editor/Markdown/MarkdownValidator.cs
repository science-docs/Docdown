using Docdown.Util;
using PandocMark.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Docdown.Editor.Markdown
{
    public static class MarkdownValidator
    {
        private static readonly Regex ReferenceRegex = new Regex("[^}]\\[([^\\]]+)\\]", RegexOptions.Compiled);
        private static Regex ForbiddenWords;

        static MarkdownValidator()
        {
            LoadForbiddenWords("German");
        }

        public static void LoadForbiddenWords(string language)
        {
            string text;
            using (var res = IOUtility.LoadResource($"Docdown.Resources.Validation.Forbidden.{language}.properties"))
            using (var reader = new StreamReader(res))
            {
                text = reader.ReadToEnd();
            }
            var split = text.Split('\r', '\n');
            var sb = new StringBuilder("(?:[\\s\\.\\[\\]]|^)(");
            sb.Append(string.Join("|", split.Where(e => !string.IsNullOrEmpty(e)).Select(e => Regex.Escape(e))));
            sb.Append(")(?:[\\s\\.\\[\\]]|$)");

            ForbiddenWords = new Regex(sb.ToString(), RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        public static IEnumerable<Issue> Validate(Block document, string text)
        {
            foreach (var block in AbstractSyntaxTree.EnumerateBlocks(document))
            {
                foreach (var issue in ValidateBlock(block, text))
                {
                    yield return issue;
                }
            }
        }

        private static IEnumerable<Issue> ValidateBlock(Block block, string text)
        {
            switch (block.Tag)
            {
                case BlockTag.ListItem:
                case BlockTag.Paragraph:
                case BlockTag.BlockQuote:
                case BlockTag.ReferenceDefinition:
                    foreach (var issue in ValidateText(block, text))
                        yield return issue;

                    break;
                case BlockTag.Meta:
                    yield break;
            }
            yield break;
        }

        private static IEnumerable<Issue> ValidateText(Block block, string text)
        {
            var sub = text.Substring(block.SourcePosition, block.SourceLength);
            var referenceMatches = ReferenceRegex.Matches(sub);
            foreach (var referenceIssue in ValidateRegex(ReferenceRegex, sub, ReferenceIssueFactory(block)))
            {
                yield return referenceIssue;
            }
            foreach (var notAllowedIssue in ValidateRegex(ForbiddenWords, sub, DefaultIssueFactory(block)))
            {
                yield return notAllowedIssue;
            }
        }

        private static Func<Group, Issue> DefaultIssueFactory(Block block)
        {
            return group =>
            {
                var index = block.SourcePosition + group.Index;
                var issue = new Issue(IssueType.Warning, index, group.Length);
                return issue;
            };
        }

        private static Func<Group, Issue> ReferenceIssueFactory(Block block)
        {
            return group =>
            {
                var index = block.SourcePosition + group.Index - 1;
                if (!block.Top.Document.ReferenceMap.ContainsKey(group.Value))
                {
                    var issue = new Issue(IssueType.Warning, index, group.Length + 2);
                    return issue;
                }
                else
                    return null;
            };
        }

        private static IEnumerable<Issue> ValidateRegex(Regex regex, string text, Func<Group, Issue> issueFactory)
        {
            var matches = regex.Matches(text);
            for (int i = 0; i < matches.Count; i++)
            {
                var match = matches[i];
                var group = match.Groups[1];
                var issue = issueFactory(group);
                if (issue != null)
                {
                    yield return issue;
                }
            }
        }
    }
}
