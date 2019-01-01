using CommonMark.Syntax;
using System.Collections.Generic;

namespace Docdown.Model
{
    public class Outline
    {
        public List<OutlineItem> Children { get; } = new List<OutlineItem>();
        public IEnumerable<Block> Source
        {
            get => source;
            set
            {
                source = value;
                CreateOutline();
            }
        }


        private IEnumerable<Block> source;

        public Outline()
        {

        }

        public Outline(IEnumerable<Block> headers)
        {
            Source = headers;
        }

        private void CreateOutline()
        {
            Children.Clear();
            Children.AddRange(OutlineItem.FromFlatList(Source));
        }
    }
}
