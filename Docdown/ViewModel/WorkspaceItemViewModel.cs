using Docdown.Controls;
using Docdown.Model;
using Docdown.Util;
using Docdown.ViewModel.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Docdown.ViewModel
{
    public class WorkspaceItemViewModel : ObservableObject<WorkspaceItem>, IExpandable<WorkspaceItemViewModel>, IComparable<WorkspaceItemViewModel>
    {
        [ChangeListener(nameof(HasChanged))]
        public string TabName => HasChanged ? Name + "*" : Name;
        public string Name => Data.FileSystemInfo.Name;
        public string FullName => Data.FileSystemInfo.FullName;

        public bool HasChanged
        {
            get => hasChanged;
            set => Set(ref hasChanged, value);
        }

        [ChangeListener(nameof(IsExpanded))]
        public string IconName
        {
            get
            {
                switch (Data?.Type)
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

        public bool CanConvert
        {
            get => canConvert && !isConverting;
            set => Set(ref canConvert, value);
        }

        public bool IsConverting
        {
            get => isConverting;
            set
            {
                Set(ref isConverting, value);
                SendPropertyUpdate(nameof(CanConvert));
            }
        }

        public string ErrorMessage
        {
            get => errorMessage;
            set => Set(ref errorMessage, value);
        }

        public string PdfPath
        {
            get => pdfPath;
            set => Set(ref pdfPath, value);
        }

        public IEnumerable<WorkspaceItemViewModel> Children
        {
            get
            {
                if (childrenCache == null)
                {
                    childrenCache = Data?.Children
                        .OrderByDescending(e => e.IsDirectory())
                        .Select(e => new WorkspaceItemViewModel(Workspace, e))
                        .ToArray();
                }
                return childrenCache;
            }
        }
        public ICommand SaveCommand => new ActionCommand(Save);
        public ICommand CloseCommand => new ActionCommand(Close);
        public ICommand ConvertCommand => new ActionCommand(Convert);
        [ChangeListener(nameof(PdfPath))]
        public ICommand PrintCommand => new PrintCommand(Name, PdfPath);

        public object View
        {
            get
            {
                if (view == null)
                {
                    view = BuildView();
                }
                return view;
            }
        }

        public OutlineViewModel Outline
        {
            get => outline;
            set
            {
                if (outline != null && value != null)
                    outline.Exchange(value);
                Set(ref outline, value);
            }
        }

        public OutlineItemViewModel SelectedOutlineItem
        {
            get; set;
        }

        public WorkspaceViewModel Workspace { get; }

        private string errorMessage;
        private string pdfPath;
        private bool isExpanded = false;
        private bool canConvert;
        private bool isConverting;
        private bool hasChanged;
        private object view;
        private WorkspaceItemViewModel[] childrenCache;
        private OutlineViewModel outline;

        public WorkspaceItemViewModel(WorkspaceViewModel workspaceViewModel, WorkspaceItem workspaceItem) : base(workspaceItem)
        {
            Workspace = workspaceViewModel;

            CanConvert = workspaceItem.IsPlainText();
        }

        public void Convert()
        {
            IsConverting = true;
            Save();
            Task.Run(() =>
            {
                try
                {
                    PdfPath = Data.Convert();
                    ErrorMessage = "";
                }
                catch (Exception e)
                {
                    ErrorMessage = ErrorUtility.GetErrorMessage(e);
                }

                IsConverting = false;
            });
        }

        public void Rename(string newName)
        {
            Data.Rename(newName);
            SendPropertyUpdate(nameof(TabName));
            SendPropertyUpdate(nameof(Name));
            SendPropertyUpdate(nameof(FullName));
        }

        public void Save()
        {
            HasChanged = false;
            Workspace.IsChanging = true;
            if (view is EditorAndViewer editorAndViewer)
            {
                File.WriteAllText(Data.FileSystemInfo.FullName, editorAndViewer.GetText());
            }
            Workspace.IsChanging = false;
        }

        public void Close()
        {
            if (Workspace.OpenItems.Contains(this))
            {
                bool isSelected = Workspace.SelectedItem == this;
                if (isSelected)
                {
                    Workspace.SelectedItem = null; 
                }

                Workspace.OpenItems.Remove(this);

                if (isSelected)
                {
                    Workspace.SelectedItem = Workspace.OpenItems.LastOrDefault();
                }
                view = null;
            }
        }

        private object BuildView()
        {
            switch (Data?.Type)
            {
                case WorkspaceItemType.Pdf:
                    return ShowPdf();
                case WorkspaceItemType.Latex:
                case WorkspaceItemType.Markdown:
                    return ShowMdEditorAndPdf();
                default:
                    return null;
            }
        }

        private DocumentViewer ShowPdf()
        {
            var docViewer = new DocumentViewer();
            try
            {
                PdfPath = Data.FileSystemInfo.FullName;
                //docViewer.Navigate(Data.FileSystemInfo.FullName);
            }
            catch (Exception e)
            {
                ErrorMessage = ErrorUtility.GetErrorMessage(e);
            }
            return docViewer;
        }

        private EditorAndViewer ShowMdEditorAndPdf()
        {
            var editorAndViewer = new EditorAndViewer();
            string allText = File.ReadAllText(Data.FileSystemInfo.FullName);
            editorAndViewer.Delay(100, () => editorAndViewer.SetText(allText));
            return editorAndViewer;
        }

        public int CompareTo(WorkspaceItemViewModel other)
        {
            return FullName.CompareTo(other?.FullName);
        }
    }
}