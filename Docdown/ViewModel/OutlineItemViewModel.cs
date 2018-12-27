using Docdown.Model;
using System.Collections.Generic;
using System.Linq;

namespace Docdown.ViewModel
{
    public class OutlineItemViewModel : ObservableObject<OutlineItem>
    {
        public OutlineItemViewModel[] Children { get; }
        
        public bool IsExpanded
        {
            get => isExpanded;
            set => Set(ref isExpanded, value);
        }

        private bool isExpanded = false;

        public OutlineItemViewModel(OutlineItem outlineItem) : base(outlineItem)
        {
            Children = Data.Children.Select(e => new OutlineItemViewModel(e)).ToArray();
        }
    }
}