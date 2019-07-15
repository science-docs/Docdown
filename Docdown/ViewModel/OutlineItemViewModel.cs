using Docdown.Model;
using Docdown.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Docdown.ViewModel
{
    public class OutlineItemViewModel : ObservableObject<OutlineItem>
    {
        public ObservableCollection<OutlineItemViewModel> Children { get; } = new ObservableCollection<OutlineItemViewModel>();

        public OutlineItemViewModel Parent { get; }

        public bool IsExpanded
        {
            get => isExpanded;
            set => Set(ref isExpanded, value);
        }

        private bool isExpanded = false;

        public OutlineItemViewModel(OutlineItem outlineItem, OutlineItemViewModel parent) : base(outlineItem ?? throw new ArgumentNullException(nameof(outlineItem)))
        {
            Parent = parent;
            foreach (var child in Data.Children.Select(e => new OutlineItemViewModel(e, this)))
            {
                Children.Add(child);
            }
        }

        public int CompareTo(OutlineItemViewModel other)
        {
            return Data.Text.CompareTo(other?.Data.Text);
        }
    }
}