using CommonMark.Syntax;
using System.Collections.Generic;

namespace Docdown.Model
{
    public class OutlineItem
    {
        public List<OutlineItem> Children { get; } = new List<OutlineItem>();
        public int Level { get; set; }
        public int TextPosition { get; set; }

        public string Text { get; set; }

        public OutlineItem()
        {

        }

        private OutlineItem(Block block)
        {
            Text = block.InlineContent.LiteralContent;
            Level = block.Heading.Level;
            TextPosition = block.SourcePosition + block.SourceLength;
        }

        public static IEnumerable<OutlineItem> FromFlatList(IEnumerable<Block> headers)
        {
            return FromFlatList(new LinkedList<Block>(headers), null);
        }

        public static IEnumerable<OutlineItem> FromFlatList(LinkedList<Block> headers, OutlineItem last = null)
        {
            if (headers.Count == 0)
                yield break;

            var first = headers.First.Value;
            headers.RemoveFirst();

            var newItem = new OutlineItem(first);
            if (IsChild(last, first.Heading.Level))
            {
                last.Children.Add(newItem);
            }
            else
            {
                yield return newItem;
            }
            foreach (var child in FromFlatList(headers, newItem))
            {
                if (IsChild(newItem, child.Level))
                {
                    newItem.Children.Add(child);
                }
                else
                {
                    yield return child;
                }
            }
        }

        private static bool IsChild(OutlineItem item, int level)
        {
            if (item == null)
                return false;
            else
                return item.Level < level;
        }
    }
}
