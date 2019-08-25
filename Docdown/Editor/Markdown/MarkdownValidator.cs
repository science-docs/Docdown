using PandocMark.Syntax;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Docdown.Editor.Markdown
{
    public static class MarkdownValidator
    {
        private static Regex ReferenceRegex = new Regex("[^}]\\[([^\\]]+)\\]", RegexOptions.Compiled);
        private static Regex NotAllowedWords = new Regex("(?:[\\s\\.\\[\\]]|^)(ich|man)(?:[\\s\\.\\[\\]]|$)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

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
            foreach (var notAllowedIssue in ValidateRegex(NotAllowedWords, sub, DefaultIssueFactory(block)))
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
