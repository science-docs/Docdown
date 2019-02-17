using Docdown.Model;
using Docdown.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Docdown.ViewModel
{
    public class OutlineItemViewModel : ObservableObject<OutlineItem>, IExpandable<OutlineItemViewModel>, IComparable<OutlineItemViewModel>
    {
        public IEnumerable<OutlineItemViewModel> Children { get; }

        public bool IsExpanded
        {
            get => isExpanded;
            set => Set(ref isExpanded, value);
        }

        private bool isExpanded = false;

        public OutlineItemViewModel(OutlineItem outlineItem) : base(outlineItem ?? throw new ArgumentNullException(nameof(outlineItem)))
        {
            Children = Data.Children.Select(e => new OutlineItemViewModel(e)).ToArray();
        }

        public int CompareTo(OutlineItemViewModel other)
        {
            return Data.Text.CompareTo(other?.Data.Text);
        }
    }
}