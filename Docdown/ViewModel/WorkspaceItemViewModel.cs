using Docdown.Model;
using System.Collections.Generic;
using System.Linq;

namespace Docdown.ViewModel
{
    public class WorkspaceItemViewModel : ObservableObject<WorkspaceItem>
    {
        [ChangeListener(nameof(IsExpanded))]
        public string IconName
        {
            get
            {
                switch (Data.Type)
                {
                    case WorkspaceItemType.Directory:
                        return IsExpanded ? "FolderOpenIcon" : "FolderIcon";
                    case WorkspaceItemType.Audio:
                        return "AudioIcon";
                    case WorkspaceItemType.Docx:
                        return "WordIcon";
                    case WorkspaceItemType.Image:
                        return "ImageIcon";
                    case WorkspaceItemType.Latex:
                        return "TexIcon";
                    case WorkspaceItemType.Pdf:
                        return "PdfIcon";
                    case WorkspaceItemType.Video:
                        return "VideoIcon";
                    case WorkspaceItemType.Markdown:
                        return "MarkdownIcon";
                    default:
                        return "DocumentIcon";
                }
            }
        }

        public bool IsExpanded
        {
            get => isExpanded;
            set => Set(ref isExpanded, value);
        }

        public IEnumerable<WorkspaceItemViewModel> Children 
            => Data.Children
                .OrderByDescending(e => e.IsDirectory())
                .Select(e => new WorkspaceItemViewModel(e));

        private bool isExpanded = false;

        public WorkspaceItemViewModel(WorkspaceItem workspaceItem) : base(workspaceItem)
        {

        }
    }
}