﻿using Docdown.Model;
using Docdown.Util;
using System;
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
            Children.Restore(other.Children,
                e => e.IsExpanded,
                e => e.Children,
                (a, b) => a.Data.Text == b.Data.Text,
                e => e.IsExpanded = true);
        }
    }
}