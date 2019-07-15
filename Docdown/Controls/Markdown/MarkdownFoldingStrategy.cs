using PandocMark.Syntax;
using Docdown.Util;
using ICSharpCode.AvalonEdit.Folding;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Docdown.Controls.Markdown
{
    public sealed class MarkdownFolding : NewFolding
    {
        public MarkdownFolding(Block block) : this(block, block.SourcePosition + block.SourceLength)
        {

        }

        public MarkdownFolding(Block block, int end) : base(block.SourcePosition, end)
        {
            const int MaxHeaderLength = 30;
            switch (block.Tag)
            {
                case BlockTag.SetextHeading:
                case BlockTag.AtxHeading:
                    string content = block.InlineContent.LiteralContent;
                    Name = new string('#', block.Heading.Level) + " ";
                    if (content.Length > MaxHeaderLength + 3)
                    {
                        Name += content.Substring(0, MaxHeaderLength) + "...";
                    }
                    else
                    {
                        Name += content;
                    }
                    break;
                case BlockTag.HtmlBlock:
                    EndOffset -= Environment.NewLine.Length;
                    Name = block.StringContent.TakeFromStart(30) + "...";
                    break;
                case BlockTag.FencedCode:
                    var data = block.FencedCodeData;
                    Name = new string(data.FenceChar, 3) + data.Info + "...";
                    break;
                case BlockTag.Meta:
                    Name = "Metadata...";
                    break;
            }
        }
    }

    public class MarkdownFoldingStrategy
    {
        private static readonly int NewLineLength = Environment.NewLine.Length;

        public void UpdateFoldings(FoldingManager manager, Block abstractSyntaxTree, string text)
        {
            manager.UpdateFoldings(GenerateFoldings(abstractSyntaxTree, text), -1);
        }

        public IEnumerable<NewFolding> GenerateFoldings(Block abstractSyntaxTree, string text)
        {
            var blocks = AbstractSyntaxTree.EnumerateBlocks(abstractSyntaxTree).ToArray();

            for (int i = 0; i < blocks.Length; i++)
            {
                var block = blocks[i];
                switch (block.Tag)
                {
                    case BlockTag.SetextHeading:
                    case BlockTag.AtxHeading:
                        yield return GenerateHeaderFolding(i, blocks, text);
                        break;
                    case BlockTag.HtmlBlock:
                        if (block.StringContent.Length > 33)
                        {
                            yield return GenerateDefaultFolding(block, text);
                        }
                        break;
                    case BlockTag.FencedCode:
                        yield return GenerateDefaultFolding(block, text);
                        break;
                    case BlockTag.Meta:
                        yield return GenerateDefaultFolding(block, text);
                        break;
                }
            }
        }

        private MarkdownFolding GenerateDefaultFolding(Block block, string text)
        {
            var folding = new MarkdownFolding(block);
            int end = folding.EndOffset;
            while (text.Length <= end || TextUtility.IsBlank(text[end]))
            {
                end -= 1;
            }
            end += 1;
            folding.EndOffset = end;
            return folding;
        }

        private MarkdownFolding GenerateHeaderFolding(int index, Block[] blocks, string text)
        {
            var header = blocks[index];
            int level = header.Heading.Level;
            int end = -1;

            for (int i = index + 1; i < blocks.Length; i++)
            {
                var newHeader = blocks[i];
                if (newHeader.Tag == BlockTag.AtxHeading || newHeader.Tag == BlockTag.SetextHeading)
                {
                    int newLevel = newHeader.Heading.Level;
                    if (newLevel <= level)
                    {
                        end = newHeader.SourcePosition - NewLineLength;
                        while (TextUtility.IsBlank(text[end]))
                        {
                            end -= 1;
                        }
                        end += 1;
                        break;
                    }
                }
            }
            if (end == -1)
            {
                end = text.Length;
            }
            return new MarkdownFolding(header, end);
        }
    }
}
