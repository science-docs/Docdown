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
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows;

using Image = System.Windows.Controls.Image;
using LibGit2Sharp;
using System.Text;

namespace Docdown.ViewModel
{
    public class WorkspaceItemViewModel : ObservableObject<IWorkspaceItem>, IExpandable<WorkspaceItemViewModel>, IComparable<WorkspaceItemViewModel>
    {
        [ChangeListener(nameof(HasChanged))]
        public string TabName => HasChanged ? Name + "*" : Name;
        public string Name
        {
            get => tempName ?? Data.Name;
            set => tempName = value;
        }
        public string RelativeName => Data.RelativeName;

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
                    case WorkspaceItemType.Directory: return IsExpanded ? "FolderOpenIcon" : "FolderIcon";
                    case WorkspaceItemType.Audio: return "AudioIcon";
                    case WorkspaceItemType.Docx: return "WordIcon";
                    case WorkspaceItemType.Image: return "ImageIcon";
                    case WorkspaceItemType.Latex: return "TexIcon";
                    case WorkspaceItemType.Pdf: return "PdfIcon";
                    case WorkspaceItemType.Video: return "VideoIcon";
                    case WorkspaceItemType.Markdown: return "MarkdownIcon";
                    default: return "DocumentIcon";
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

        public string PdfPath
        {
            get => pdfPath;
            set => Set(ref pdfPath, value);
        }
        
        public FileStatus FileStatus
        {
            get => fileStatus;
            set => Set(ref fileStatus, value);
        }

        public SearchViewModel Search { get; private set; }

        public IEnumerable<WorkspaceItemViewModel> Children
        {
            get
            {
                if (childrenCache is null)
                {
                    childrenCache = Data?.Children
                        .OrderByDescending(e => e.IsDirectory)
                        .ThenBy(e => e.Name)
                        .Select(e => new WorkspaceItemViewModel(Workspace, this, e))
                        .ToArray();
                }
                return childrenCache;
            }
        }
        public ICommand SaveCommand => new ActionCommand(Save);
        public ICommand CloseCommand => new ActionCommand(Close);
        public ICommand ConvertCommand => new ActionCommand(Convert);
        public ICommand StopConvertCommand => new ActionCommand(StopConvert);
        [ChangeListener(nameof(PdfPath))]
        public ICommand PrintCommand => string.IsNullOrEmpty(PdfPath) ? null : new PrintCommand(Workspace, Name, PdfPath);
        public ICommand RenameCommand => new ActionCommand(() => IsNameChanging = true);
        public ICommand CancelNameChangeCommand => new ActionCommand(CancelNameChange);
        public ICommand NameChangeEndCommand => new ActionCommand(NameChangeEnd);
        public ICommand SelectItemCommand => new ActionCommand(SelectItem);
        public ICommand DeleteCommand => new ActionCommand(Delete);
        [ChangeListener(nameof(RelativeName))]
        public ICommand OpenInExplorerCommand => new OpenExplorerCommand(RelativeName);
        public ICommand NewFileCommand => new CreateNewFileCommand(this);
        public ICommand ExistingFileCommand => new AddExistingFileCommand(this);

        public object View
        {
            get
            {
                if (view is null)
                {
                    view = BuildView();
                    if (view is IEditor editor)
                    {
                        Search = new SearchViewModel(editor);
                    }
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

        public int? WordCount
        {
            get => wordCount;
            set => Set(ref wordCount, value);
        }

        public bool IsDirectory => Data.IsDirectory;
        public bool IsFile => Data.IsFile;

        public WorkspaceViewModel Workspace { get; }
        public WorkspaceItemViewModel Parent { get; }

        private string pdfPath;
        private bool isExpanded = false;
        private bool canConvert;
        private bool isConverting;
        private bool hasChanged;
        private bool isNameChanging;
        private string tempName;
        private object view;
        private int? wordCount;
        private WorkspaceItemViewModel[] childrenCache;
        private OutlineViewModel outline;
        private CancelToken converterToken;
        private FileStatus fileStatus;

        public WorkspaceItemViewModel(WorkspaceViewModel workspaceViewModel, WorkspaceItemViewModel parent, IWorkspaceItem workspaceItem) 
            : base(workspaceItem ?? throw new ArgumentNullException(nameof(workspaceItem)))
        {
            Workspace = workspaceViewModel ?? throw new ArgumentNullException(nameof(workspaceViewModel));
            Parent = parent;

            CanConvert = IsPlainText(workspaceItem);
        }

        private bool IsPlainText(IWorkspaceItem item)
        {
            switch (item.Type)
            {
                case WorkspaceItemType.Text:
                case WorkspaceItemType.Latex:
                case WorkspaceItemType.Markdown:
                    return true;
            }
            return false;
        }

        public void UpdateFileStatus()
        {
            // TODO: Repair this
            //var repo = Workspace.Data.Repository;
            //if (repo != null)
            //{
            //    if (Data.Type != WorkspaceItemType.Directory)
            //    {
            //        FileStatus = repo.RetrieveStatus(GitName());
            //    }
            //    else
            //    {
            //        foreach (var child in Children)
            //        {
            //            child.UpdateFileStatus();
            //        }
            //        if (Children.All(e => e.FileStatus == FileStatus.Ignored))
            //        {
            //            FileStatus = FileStatus.Ignored;
            //        }
            //    }
            //}
            //else
            //{
            //    FileStatus = FileStatus.Unaltered;
            //}
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

        public WorkspaceItemViewModel AddChild(IWorkspaceItem child)
        {
            if (child is null)
                throw new ArgumentNullException(nameof(child));

            child.Parent = Data;

            var childViewModel = new WorkspaceItemViewModel(Workspace, this, child);
            AddChild(childViewModel);
            return childViewModel;
        }

        public void AddChild(WorkspaceItemViewModel child)
        {
            if (child is null || child.Data is null)
            {
                throw new ArgumentNullException(nameof(child));
            }

            // TODO: Repair this
            //if (Workspace.Data.Repository != null)
            //{
            //    child.FileStatus = FileStatus.NewInWorkdir;
            //}
            if (!Data.Children.Contains(child.Data))
            {
                Data.Children.Add(child.Data);
            }
            childrenCache = childrenCache.Concat(child).OrderByDescending(e => e.IsDirectory).ThenBy(e => e.Name).ToArray();
            Workspace.RefreshExplorer();
            SendPropertyUpdate(nameof(Children));
        }

        public void StopConvert()
        {
            if (!IsConverting || converterToken is null)
                return;

            converterToken.Cancel();
            Workspace.Messages.Warning(Language.Current.Get("Workspace.Compilation.Cancelled"));
        }

        public void Convert()
        {
            if (!CanConvert)
                return;
            
            converterToken = new CancelToken();
            Workspace.Messages.Working(Language.Current.Get("Workspace.Compilation.Running"));
            IsConverting = true;
            Save();
            Task.Run(() =>
            {
                var watch = Stopwatch.StartNew();
                try
                {
                    PdfPath = string.Empty;
                    PdfPath = Data.Convert(converterToken);
                    watch.Stop();
                    Workspace.Messages.Success(Language.Current.Get("Workspace.Compilation.Success", watch.Elapsed.Seconds, watch.Elapsed.Milliseconds));
                }
                catch (Exception e)
                {
                    if (!converterToken.IsCanceled)
                    {
                        Workspace.Messages.Error(ErrorUtility.GetErrorMessage(e));
                    }
                }
                watch.Stop();

                IsConverting = false;
            });
        }

        public void Delete()
        {
            var lang = Language.Current;
            if (!IsNameChanging && ShowMessage(lang.Get("Workspace.Delete.File.Title"), lang.Get("Workspace.Delete.File.Text", Name), MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                RemoveFromOpenItems();
                Workspace.IgnoreChange = true;
                Data.Delete();
                Workspace.IgnoreChange = false;
                if (Parent != null)
                {
                    Parent.childrenCache = Parent.childrenCache.Except(this).ToArray();
                }
                string hash = IOUtility.GetHashFile(RelativeName);
                if (File.Exists(hash))
                {
                    try
                    {
                        File.Delete(hash);
                    }
                    catch
                    {
                        // Could not delete temp file. This is fine
                    }
                }
                Workspace.RefreshExplorer();
                Workspace.SendPropertyUpdate(nameof(Children));
            }
        }

        private void RemoveFromOpenItems()
        {
            if (Workspace.SelectedItem == this)
            {
                Workspace.SelectedItem = null;
            }
            if (Workspace.PreSelectedItem == this)
            {
                Workspace.PreSelectedItem = null;
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
                UpdateFileStatus();
            }
            catch
            {
                Trace.WriteLine("Could not rename file to: " + newName);
            }
            Workspace.IgnoreChange = false;
            SendPropertyUpdate(nameof(TabName));
            SendPropertyUpdate(nameof(Name));
            SendPropertyUpdate(nameof(RelativeName));
        }

        public void Save()
        {
            if (view is IEditor editorWrapper)
            {
                HasChanged = false;
                Workspace.IgnoreChange = true;
                Data.Save(editorWrapper.Editor.Text);
                Workspace.IgnoreChange = false;
                UpdateFileStatus();
            }
        }

        public void Close()
        {
            if (Workspace.OpenItems.Contains(this))
            {
                if (HasChanged)
                {
                    var lang = Language.Current;
                     switch (ShowMessage(lang.Get("Workspace.Save.File.Title"), lang.Get("Workspace.Save.File.Text"), MessageBoxButton.YesNoCancel))
                     {
                        case MessageBoxResult.Yes:
                            Save();
                            break;
                        case MessageBoxResult.Cancel:
                            return;
                     }
                }

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

        private string GitName()
        {
            var rel = Data.RelativeName;
            rel = rel.Substring(rel.IndexOf('/') + 1);
            return rel;
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
                PdfPath = RelativeName;
            }
            catch (Exception e)
            {
                Workspace.Messages.Error(ErrorUtility.GetErrorMessage(e));
            }
            return docViewer;
        }

        private EditorAndViewer ShowMdEditorAndPdf()
        {
            var editorAndViewer = new EditorAndViewer();
            var temp = IOUtility.GetHashFile(RelativeName);
            if (File.Exists(temp))
            {
                PdfPath = temp;
            }
            string allText = ReadText(Data.Read());
            editorAndViewer.Delay(100, () => editorAndViewer.Editor.Text = allText);
            return editorAndViewer;
        }

        private MarkdownEditor ShowEditor()
        {
            var editor = new MarkdownEditor();
            string allText = ReadText(Data.Read());
            editor.Delay(100, () => editor.Editor.Text = allText);
            return editor;
        }

        private string ReadText(byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }

        private Image ShowImage()
        {
            var image = new Image();
            try
            {
                
                using (var ms = new MemoryStream())
                {
                    var bytes = Data.Read();
                    ms.Write(bytes, 0, bytes.Length);
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = ms;
                    bitmap.EndInit();
                    image.Source = bitmap;
                }
            }
            catch
            {
                Workspace.Messages.Error(Language.Current.Get("Workspace.Image.Failed"));
            }
            
            return image;
        }

        private void SelectItem()
        {
            Workspace.SelectedItem = this;
        }
        
        public int CompareTo(WorkspaceItemViewModel other)
        {
            return RelativeName.CompareTo(other?.RelativeName);
        }
    }
}