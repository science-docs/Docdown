using PandocMark;
using PandocMark.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Docdown.Editor.Markdown
{
    public static class AbstractSyntaxTree
    {
        public static PandocMarkSettings CommonMarkSettings { get; }

        static AbstractSyntaxTree()
        {
            CommonMarkSettings = PandocMarkSettings.Default.Clone();
            CommonMarkSettings.TrackSourcePosition = true;
        }

        public static int CountWords(Block ast, string text)
        {
            int words = 0;
            foreach (var block in EnumerateBlocks(ast))
            {
                switch (block.Tag)
                {
                    case BlockTag.AtxHeading:
                    case BlockTag.SetextHeading:
                    case BlockTag.Paragraph:
                    case BlockTag.BlockQuote:
                        words += CountSingleWords(block, text);
                        break;
                }
                // The start of an atxheading is counted twice
                if (block.Tag == BlockTag.AtxHeading)
                {
                    words--;
                }
            }

            return words;
        }

        private static readonly HashSet<char> EndCharacters = new HashSet<char>(" \r\n\t\"\\-_+*#@$%&/(){}[]?^°");

        private static int CountSingleWords(Block block, string text)
        {
            var start = block.SourcePosition + 1;
            var end = Math.Min(text.Length, start + block.SourceLength) - 1;

            // The algorithm always skips the first word, so we start at 1
            int words = 1;
            for (int i = start; i < end; i++)
            {
                var current = text[i];
                int j = i + 1;
                var next = text[j];
                if (EndCharacters.Contains(current) && !EndCharacters.Contains(next))
                {
                    words++;
                    i++;
                }
            }
            return words;
        }

        public static Block GenerateAbstractSyntaxTree(string text)
        {
            using (var reader = new StringReader(Normalize(text)))
            {
                var ast = PandocMarkConverter.ProcessStage1(reader, CommonMarkSettings);
                PandocMarkConverter.ProcessStage2(ast, CommonMarkSettings);
                return ast;
            }
        }

        private static string Normalize(string value) { return value.Replace('→', '\t').Replace('␣', ' '); }

        public static readonly Dictionary<BlockTag, Func<Theme, Highlight>> BlockHighlighter = new Dictionary<BlockTag, Func<Theme, Highlight>>
        {
            {BlockTag.AtxHeading, t => t.HighlightHeading},
            {BlockTag.SetextHeading, t => t.HighlightHeading},
            {BlockTag.BlockQuote, t => t.HighlightBlockQuote},
            {BlockTag.ListItem, t => t.HighlightStrongEmphasis},
            {BlockTag.FencedCode, t => t.HighlightBlockCode},
            {BlockTag.IndentedCode, t => t.HighlightBlockCode},
            {BlockTag.HtmlBlock, t => t.HighlightBlockCode},
            {BlockTag.ReferenceDefinition, t => t.HighlightLink}
        };

        public static readonly Dictionary<InlineTag, Func<Theme, Highlight>> InlineHighlighter = new Dictionary<InlineTag, Func<Theme, Highlight>>
        {
            {InlineTag.Code, t => t.HighlightInlineCode},
            {InlineTag.Emphasis, t => t.HighlightEmphasis},
            {InlineTag.Strong, t => t.HighlightStrongEmphasis},
            {InlineTag.Link, t => t.HighlightLink},
            {InlineTag.Image, t => t.HighlightImage},
            {InlineTag.RawHtml, t => t.HighlightBlockCode},
            {InlineTag.Tex, t => t.HighlightTex},
            {InlineTag.Task, t => t.HighlightTask}
        };

        public static IEnumerable<Block> EnumerateHeader(Block ast)
        {
            return EnumerateBlocks(ast).Where(e => e.Tag == BlockTag.AtxHeading || e.Tag == BlockTag.SetextHeading);
        }

        public static IEnumerable<Block> EnumerateSpanningBlocks(Block ast, int startOffset, int endOffset)
        {
            return EnumerateBlocks(ast.FirstChild)
                .Where(b => b.SourcePosition + b.SourceLength > startOffset)
                .TakeWhile(b => b.SourcePosition < endOffset);
        }

        public static IEnumerable<Block> EnumerateBlocks(Block block)
        {
            if (block is null) yield break;
            var stack = new Stack<Block>();
            stack.Push(block);
            while (stack.Any())
            {
                var next = stack.Pop();
                yield return next;
                if (next.NextSibling != null) stack.Push(next.NextSibling);
                if (next.FirstChild != null) stack.Push(next.FirstChild);
            }
        }

        public static IEnumerable<Inline> EnumerateInlines(Inline inline)
        {
            if (inline is null) yield break;
            var stack = new Stack<Inline>();
            stack.Push(inline);
            while (stack.Any())
            {
                var next = stack.Pop();
                yield return next;
                if (next.NextSibling != null) stack.Push(next.NextSibling);
                if (next.FirstChild != null) stack.Push(next.FirstChild);
            }
        }

        public static bool PositionSafeForSmartLink(Block ast, int start, int length)
        {
            if (ast is null) return true;
            var end = start + length;
            var blockTags = new[] { BlockTag.FencedCode, BlockTag.HtmlBlock, BlockTag.IndentedCode, BlockTag.ReferenceDefinition };
            var inlineTags = new[] { InlineTag.Code, InlineTag.Link, InlineTag.RawHtml, InlineTag.Image };
            var lastBlockTag = BlockTag.Document;

            foreach (var block in EnumerateBlocks(ast.FirstChild))
            {
                if (block.SourcePosition + block.SourceLength < start)
                {
                    lastBlockTag = block.Tag;
                    continue;
                }

                if (block.SourcePosition >= end) return !blockTags.Any(tag => tag == lastBlockTag);
                if (blockTags.Any(tag => tag == block.Tag)) return false;

                return !EnumerateInlines(block.InlineContent)
                    .TakeWhile(il => il.SourcePosition < end)
                    .Where(il => il.SourcePosition + il.SourceLength > start)
                    .Any(il => inlineTags.Any(tag => tag == il.Tag));
            }
            return true;
        }
    }
}
