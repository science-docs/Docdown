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
            if (ast == null)
                throw new ArgumentNullException(nameof(ast));
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            int words = EnumerateBlocks(ast).AsParallel().Sum(e => CountBlockWords(e, text));

            return words;
        }

        private static int CountBlockWords(Block block, string text)
        {
            int words = 0;
            switch (block.Tag)
            {
                case BlockTag.AtxHeading:
                case BlockTag.SetextHeading:
                case BlockTag.Paragraph:
                    words = CountSingleWords(block, text);
                    break;
            }
            // The start of an atxheading is counted twice
            if (block.Tag == BlockTag.AtxHeading && words > 0)
            {
                words--;
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
            if (text == null)
                throw new ArgumentNullException(nameof(text));

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
            {BlockTag.AtxHeading, t => t["Heading"]},
            {BlockTag.SetextHeading, t => t["Heading"]},
            {BlockTag.BlockQuote, t => t["BlockQuote"]},
            {BlockTag.ListItem, t => t["StrongEmphasis"]},
            {BlockTag.FencedCode, t => t["BlockCode"]},
            {BlockTag.IndentedCode, t => t["BlockCode"]},
            {BlockTag.HtmlBlock, t => t["BlockCode"]},
            {BlockTag.ReferenceDefinition, t => t["Link"]}
        };

        public static readonly Dictionary<InlineTag, Func<Theme, Highlight>> InlineHighlighter = new Dictionary<InlineTag, Func<Theme, Highlight>>
        {
            {InlineTag.Code, t => t["InlineCode"]},
            {InlineTag.Emphasis, t => t["Emphasis"]},
            {InlineTag.Strong, t => t["StrongEmphasis"]},
            {InlineTag.Link, t => t["Link"]},
            {InlineTag.Image, t => t["Image"]},
            {InlineTag.RawHtml, t => t["BlockCode"]},
            {InlineTag.Tex, t => t["Tex"]},
            {InlineTag.Task, t => t["Task"]}
        };

        public static IEnumerable<Block> EnumerateHeader(Block ast)
        {
            if (ast == null)
                throw new ArgumentNullException(nameof(ast));

            return EnumerateBlocks(ast).Where(e => e.Tag == BlockTag.AtxHeading || e.Tag == BlockTag.SetextHeading);
        }

        public static IEnumerable<Block> EnumerateSpanningBlocks(Block ast, int startOffset, int endOffset)
        {
            if (ast == null)
                throw new ArgumentNullException(nameof(ast));

            return EnumerateBlocks(ast.FirstChild)
                .Where(b => b.SourcePosition + b.SourceLength > startOffset)
                .TakeWhile(b => b.SourcePosition < endOffset);
        }

        public static Block SpanningBlock(Block ast, int index)
        {
            return DeepestBlock(EnumerateSpanningBlocks(ast, index, index));
        }

        private static Block DeepestBlock(IEnumerable<Block> blocks)
        {
            int maxDepth = -1;
            Block deepest = null;
            foreach (var block in blocks)
            {
                var depth = Depth(block);
                if (depth > maxDepth)
                {
                    deepest = block;
                    maxDepth = depth;
                }
            }

            return deepest;
        }

        private static int Depth(Block block)
        {
            int depth = 0;
            while (block.Parent != null)
            {
                depth++;
                block = block.Parent;
            }
            return depth;
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

        public static Inline SpanningInline(Block ast, int index)
        {
            return SpanningBlockInline(SpanningBlock(ast, index), index);
        }

        public static Inline SpanningInline(Block ast, int index, out Block block)
        {
            block = SpanningBlock(ast, index);
            return SpanningBlockInline(block, index);
        }

        public static Inline SpanningBlockInline(Block block, int index)
        {
            if (block == null)
                return null;

            var inline = EnumerateInlines(block.InlineContent).Where(e => e.Tag != InlineTag.String && e.SourcePosition + e.SourceLength > index && index >= e.SourcePosition);
            return inline.OrderByDescending(e => e.SourcePosition).FirstOrDefault();
        }
    }
}
