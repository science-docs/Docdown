using Docdown.Util;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using PM = System.Tuple<System.Text.RegularExpressions.Regex, System.Action<System.Text.RegularExpressions.Match>>;

namespace Docdown.Editor.Markdown
{
    public static class ListInputHandler
    {
        public static readonly Regex UnorderedListCheckboxPattern = new Regex(@"^[ ]{0,3}[-\*\+][ ]{1,3}\[[ xX]\](?=[ ]{1,3}\S)", RegexOptions.Compiled);
        public static readonly Regex UnorderedListCheckboxEndPattern = new Regex(@"^[ ]{0,3}[-\*\+][ ]{1,3}\[[ xX]\]\s*", RegexOptions.Compiled);
        public static readonly Regex OrderedListPattern = new Regex(@"^[ ]{0,3}([0-9]+|[a-z#])(\.|\))(?=[ ]{1,3}\S)", RegexOptions.Compiled);
        public static readonly Regex OrderedListEndPattern = new Regex(@"^[ ]{0,3}([0-9]+|[a-z#])(\.|\))(?=[ ]{1,3}\s*)", RegexOptions.Compiled);
        public static readonly Regex UnorderedListPattern = new Regex(@"^[ ]{0,3}[-\*\+\•](?=[ ]{1,3}\S)", RegexOptions.Compiled);
        public static readonly Regex UnorderedListEndPattern = new Regex(@"^[ ]{0,3}[-\*\+\•](?=[ ]{1,3}\s*)", RegexOptions.Compiled);
        public static readonly Regex BlockQuotePattern = new Regex(@"^(([ ]{0,4}>)+)[ ]{0,4}.{2}", RegexOptions.Compiled);
        public static readonly Regex BlockQuoteEndPattern = new Regex(@"^([ ]{0,4}>)+[ ]{0,4}\s*", RegexOptions.Compiled);

        public static void AdjustList(TextEditor editor, bool lineCountIncreased)
        {
            var document = editor.Document;
            var line = document.GetLineByOffset(editor.SelectionStart)?.PreviousLine;
            if (line is null) return;
            var text = document.GetText(line.Offset, line.Length);

            // A poor mans pattern matcher

            void match(string txt, IEnumerable<PM> patterns)
            {
                var firstMatch = patterns
                    .Select(pattern => new { Match = pattern.Item1.Match(txt), Action = pattern.Item2 })
                    .FirstOrDefault(ma => ma.Match.Success);

                firstMatch?.Action(firstMatch.Match);
            }

            void ifIncreased(Action func)
            {
                if (lineCountIncreased) func();
            }

            void atEndOfList(string symbol, Action action)
            {
                var lineText = document.GetText(line.Offset, line.Length);
                if (lineText.StartsWith(symbol)) action();
            }

            document.BeginUpdate();

            match(text, new[]
            {
                new PM(UnorderedListCheckboxPattern,
                    m => ifIncreased(() => document.Insert(editor.SelectionStart, m.Groups[0].Value.TrimStart() + " "))),

                new PM(UnorderedListCheckboxEndPattern,
                    m => atEndOfList(m.Groups[0].Value , () => document.Remove(line))),

                new PM(UnorderedListPattern,
                    m => ifIncreased(() => document.Insert(editor.SelectionStart, m.Groups[0].Value.TrimStart() + " "))),

                new PM(UnorderedListEndPattern,
                    m =>  atEndOfList(m.Groups[0].Value , () => document.Remove(line))),

                new PM(OrderedListPattern, m =>
                {
                    var numberText = m.Groups[1].Value;
                    var type = m.Groups[2].Value;
                    var num = "#";
                    bool isNumber = false;
                    if (int.TryParse(numberText, out int number))
                    {
                        num = (++number).ToString();
                        isNumber = true;
                    }
                    else if (numberText.Length == 1 && char.IsLower(numberText[0]))
                    {
                        num = TextUtility.Increase(numberText[0]);
                    }
                    if (lineCountIncreased && !string.IsNullOrEmpty(num))
                    {
                        document.Insert(editor.SelectionStart, num + type + " ");
                        line = line.NextLine;
                    }
                    if (isNumber)
                    {
                        RenumberOrderedList(document, line, number);
                    }
                }),

                new PM(OrderedListEndPattern,
                    m =>  atEndOfList(m.Groups[0].Value , () => document.Remove(line))),

                new PM(BlockQuotePattern,
                    m => ifIncreased(() => document.Insert(editor.SelectionStart, m.Groups[1].Value.TrimStart()))),

                new PM(BlockQuoteEndPattern,
                    m =>  atEndOfList(m.Groups[0].Value , () => document.Remove(line)))
            });

            document.EndUpdate();
        }

        private static void RenumberOrderedList(
            IDocument document,
            DocumentLine line,
            int number)
        {
            if (line is null) return;
            while ((line = line.NextLine) != null)
            {
                number += 1;
                var text = document.GetText(line.Offset, line.Length);
                var match = OrderedListPattern.Match(text);
                if (match.Success == false) break;
                var group = match.Groups[1];
                var currentNumber = int.Parse(group.Value);
                if (currentNumber != number)
                {
                    document.Replace(line.Offset + group.Index, group.Length, number.ToString());
                }
            }
        }
    }
}
