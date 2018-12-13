using Docdown.Model;
using System.Collections.Generic;
using System.Linq;

namespace Docdown.ViewModel
{
    public class WorkspaceItemViewModel : ObservableObject<WorkspaceItem>
    {
        public string IconName
        {
            get
            {
                switch (Data.Type)
                {
                    case WorkspaceItemType.Directory:
                        return IsExpanded ? "FolderOpenIcon" : "FolderIcon";
                    default:
                        return "DocumentIcon";
                }
            }
        }

        public bool IsExpanded
        {
            get => isExpanded;
            set
            {
                Set(ref isExpanded, value);
                if (Data.Type == WorkspaceItemType.Directory)
                    SendPropertyUpdate(nameof(IconName));
            }
        }

        public IEnumerable<WorkspaceItemViewModel> Children
        {
            get => Data.Children.Select(e => new WorkspaceItemViewModel(e));
        }

        private bool isExpanded = false;

        public WorkspaceItemViewModel(WorkspaceItem workspaceItem) : base(workspaceItem)
        {

        }
    }
}