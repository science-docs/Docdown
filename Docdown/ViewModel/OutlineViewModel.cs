using Docdown.Model;
using Docdown.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Docdown.ViewModel
{
    public class OutlineViewModel : ObservableObject<Outline>
    {
        public ObservableCollection<OutlineItemViewModel> Children { get; } = new ObservableCollection<OutlineItemViewModel>();

        public Action<int> JumpTo { get; set; }

        public OutlineViewModel(Outline outline, Action<int> jumpTo) : base(outline ?? throw new ArgumentNullException(nameof(outline)))
        {
            JumpTo = jumpTo;
            foreach (var child in Data.Children.Select(e => new OutlineItemViewModel(e, null)))
            {
                Children.Add(child);
            }
        }

        public void Exchange(OutlineViewModel other)
        {
            var ownFlattened = Flatten().ToArray();
            var otherFlattened = other.Flatten().ToArray();

            Children.Clear();

            foreach (var otherItem in otherFlattened)
            {
                if (ExistsExpanded(otherItem, ownFlattened))
                {
                    otherItem.IsExpanded = true;
                }

                if (otherItem.Parent == null)
                {
                    Children.Add(otherItem);
                }
            }
        }

        private bool ExistsExpanded(OutlineItemViewModel item, OutlineItemViewModel[] ownItems)
        {
            return ownItems.FirstOrDefault(e => e.Data.Text == item.Data.Text || e.Data.TextPosition == item.Data.TextPosition)?.IsExpanded ?? false;
        }

        private IEnumerable<OutlineItemViewModel> Flatten()
        {
            foreach (var child in Children)
            {
                yield return child;
                foreach (var item in Flatten(child))
                {
                    yield return item;
                }
            }
        }

        private IEnumerable<OutlineItemViewModel> Flatten(OutlineItemViewModel item)
        {
            foreach (var child in item.Children)
            {
                yield return child;
                foreach (var flatChild in Flatten(child))
                {
                    yield return flatChild;
                }
            }
        }
    }
}