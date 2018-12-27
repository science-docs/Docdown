using Docdown.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Docdown.ViewModel
{
    public class OutlineViewModel : ObservableObject<Outline>
    {
        public OutlineItemViewModel[] Children { get; }

        public Action<int> JumpTo { get; set; }

        public OutlineViewModel(Outline outline, Action<int> jumpTo) : base(outline)
        {
            JumpTo = jumpTo;
            Children = Data.Children.Select(e => new OutlineItemViewModel(e)).ToArray();
        }

        public void Exchange(OutlineViewModel other)
        {
            Exchange(Children, other.Children);
        }

        private void Exchange(IEnumerable<OutlineItemViewModel> own, IEnumerable<OutlineItemViewModel> others)
        {
            foreach (var child in own)
            {
                if (child.IsExpanded)
                {
                    var other = SearchInOther(others, child.Data.Text);
                    if (other != null)
                    {
                        other.IsExpanded = true;
                    }
                    Exchange(child.Children, others);
                }
            }
        }

        private OutlineItemViewModel SearchInOther(IEnumerable<OutlineItemViewModel> others, string name)
        {
            OutlineItemViewModel found = null;
            foreach (var item in others)
            {
                if (item.Data.Text == name)
                {
                    return item;
                }
                else
                {
                    found = SearchInOther(item.Children, name);
                }
            }
            return found;
        }
    }
}