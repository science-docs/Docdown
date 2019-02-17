using Docdown.Controls;
using Docdown.Model;
using Docdown.Util;
using Docdown.ViewModel.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Image = System.Windows.Controls.Image;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows;

namespace Docdown.ViewModel
{
    public class WorkspaceItemViewModel : ObservableObject<WorkspaceItem>, IExpandable<WorkspaceItemViewModel>, IComparable<WorkspaceItemViewModel>
    {
        [ChangeListener(nameof(HasChanged))]
        public string TabName => HasChanged ? Name + "*" : Name;
        public string Name
        {
            get => tempName ?? Data.FileSystemInfo.Name;
            set => tempName = value;
        }
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

        public bool IsNameChanging
        {
            get => isNameChanging;
            set => Set(ref isNameChanging, value);
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
                        .Select(e => new WorkspaceItemViewModel(Workspace, this, e))
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
        public ICommand CancelNameChangeCommand => new ActionCommand(CancelNameChange);
        public ICommand NameChangeEndCommand => new ActionCommand(NameChangeEnd);
        public ICommand SelectItemCommand => new ActionCommand(SelectItem);
        public ICommand DeleteCommand => new ActionCommand(Delete);
        public ICommand NewFileCommand => new CreateNewFileCommand(this);

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

        public bool IsDirectory => Data.IsDirectory();
        public bool IsFile => !IsDirectory;

        public WorkspaceViewModel Workspace { get; }
        public WorkspaceItemViewModel Parent { get; }

        private string errorMessage;
        private string pdfPath;
        private bool isExpanded = false;
        private bool canConvert;
        private bool isConverting;
        private bool hasChanged;
        private bool isNameChanging;
        private string tempName;
        private object view;
        private WorkspaceItemViewModel[] childrenCache;
        private OutlineViewModel outline;

        public WorkspaceItemViewModel(WorkspaceViewModel workspaceViewModel, WorkspaceItemViewModel parent, WorkspaceItem workspaceItem) 
            : base(workspaceItem ?? throw new ArgumentNullException(nameof(workspaceItem)))
        {
            Workspace = workspaceViewModel ?? throw new ArgumentNullException(nameof(workspaceViewModel));
            Parent = parent;

            CanConvert = workspaceItem.IsPlainText();
        }

        public void InsertTextAtPosition(string text)
        {
            if (string.IsNullOrEmpty(text))
                return;

            if (view is IEditor editorWrapper)
            {
                editorWrapper.Editor.TextArea.Selection.ReplaceSelectionWithText(text);
            }
        }

        public void AddChild(WorkspaceItem child)
        {
            if (child == null)
                throw new ArgumentNullException(nameof(child));

            AddChild(new WorkspaceItemViewModel(Workspace, this, child));
        }

        public void AddChild(WorkspaceItemViewModel child)
        {
            if (child == null || child.Data == null)
            {
                throw new ArgumentNullException(nameof(child));
            }

            Data.Children.Add(child.Data);
            childrenCache = childrenCache.Concat(new [] { child }).OrderByDescending(e => e.IsDirectory).ToArray();
            SendPropertyUpdate(nameof(Children));
        }

        public void Convert()
        {
            ErrorMessage = "";
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

        public void Delete()
        {
            if (!IsNameChanging && ShowMessage("Delete file", $"Are you sure you want to delete \"{Name}\"?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                RemoveFromOpenItems();
                Workspace.IgnoreChange = true;
                Data.Delete();
                Workspace.IgnoreChange = false;
                if (Parent != null)
                {
                    Parent.childrenCache = Parent.childrenCache.Except(this).ToArray();
                }
                Workspace.SendPropertyUpdate(nameof(Children));
            }
        }

        private void RemoveFromOpenItems()
        {
            if (Workspace.SelectedItem == this)
            {
                Workspace.SelectedItem = null;
            }
            if (Workspace.SelectedWorkspaceItem == this)
            {
                Workspace.SelectedWorkspaceItem = null;
            }
            Workspace.OpenItems.Remove(this);
            if (childrenCache != null)
            {
                foreach (var child in Children)
                {
                    child.RemoveFromOpenItems();
                }
            }
        }

        public void Rename(string newName)
        {
            Workspace.IgnoreChange = true;
            try
            {
                Data.Rename(newName);
            }
            catch
            {
                Trace.WriteLine("Could not rename file to: " + newName);
            }
            Workspace.IgnoreChange = false;
            SendPropertyUpdate(nameof(TabName));
            SendPropertyUpdate(nameof(Name));
            SendPropertyUpdate(nameof(FullName));
        }

        public void Save()
        {
            HasChanged = false;
            Workspace.IgnoreChange = true;
            if (view is IEditor editorWrapper)
            {
                File.WriteAllText(Data.FileSystemInfo.FullName, editorWrapper.Editor.Text);
            }
            Workspace.IgnoreChange = false;
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

        private void CancelNameChange()
        {
            tempName = null;
            IsNameChanging = false;
        }

        private void NameChangeEnd()
        {
            Rename(tempName);
            CancelNameChange();
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
                case WorkspaceItemType.Bibliography:
                case WorkspaceItemType.Text:
                    return ShowEditor();
                case WorkspaceItemType.Image:
                    return ShowImage();
                default:
                    return null;
            }
        }

        private DocumentViewer ShowPdf()
        {
            var docViewer = new DocumentViewer();
            try
            {
                PdfPath = FullName;
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
            string allText = File.ReadAllText(FullName);
            editorAndViewer.Delay(100, () => editorAndViewer.Editor.Text = allText);
            return editorAndViewer;
        }

        private MarkdownEditor ShowEditor()
        {
            var editor = new MarkdownEditor();
            string allText = File.ReadAllText(FullName);
            editor.Delay(100, () => editor.Editor.Text = allText);
            return editor;
        }

        private Image ShowImage()
        {
            var image = new Image();
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(FullName, UriKind.Absolute);
            bitmap.EndInit();
            image.Source = bitmap;
            return image;
        }

        private void SelectItem()
        {
            Workspace.SelectedItem = this;
        }
        
        public int CompareTo(WorkspaceItemViewModel other)
        {
            return FullName.CompareTo(other?.FullName);
        }
    }
}