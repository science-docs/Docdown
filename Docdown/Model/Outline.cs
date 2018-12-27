using CommonMark.Syntax;
using System.Collections.Generic;

namespace Docdown.Model
{
    public class Outline
    {
        public List<OutlineItem> Children { get; } = new List<OutlineItem>();

        public Outline()
        {

        }

        public Outline(IEnumerable<Block> headers)
        {
            Children.AddRange(OutlineItem.FromFlatList(headers));
        }
    }
}
