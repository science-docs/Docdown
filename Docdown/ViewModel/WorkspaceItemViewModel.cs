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
using System.Text;
using Docdown.Editor;
using Docdown.Editor.Commands;
using System.Collections.ObjectModel;

namespace Docdown.ViewModel
{
    public class WorkspaceItemViewModel : ObservableObject<IWorkspaceItem>, IExpandable<WorkspaceItemViewModel>, IComparable<WorkspaceItemViewModel>
    {
        [ChangeListener(nameof(HasChanged), nameof(Name))]
        public string TabName => HasChanged ? Name + "*" : Name;
        public string Name
        {
            get => tempName ?? Data.Name;
            set => tempName = value;
        }
        [ChangeListener(nameof(Name))]
        public string RelativeName => Data.RelativeName;
        [ChangeListener(nameof(Name))]
        public string FullName => Data.FullName;

        public bool HasChanged
        {
            get => hasChanged;
            set => Set(ref hasChanged, value);
        }

        [ChangeListener(nameof(IsExpanded), nameof(Name))]
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
                    case WorkspaceItemType.Web: return "WebIcon";
                    case WorkspaceItemType.Bibliography: return "BibliographyIcon";
                    default: return "DocumentIcon";
                }
            }
        }

        public bool IsCompiled
        {
            get => isCompiled;
            set => Set(ref isCompiled, value);
        }

        public bool IsExpanded
        {
            get => isExpanded;
            set => Set(ref isExpanded, value);
        }

        [ChangeListener(nameof(IsConverting), nameof(HasValidationErrors), nameof(IsConvertable))]
        public bool CanConvert => IsConvertable && !IsConverting && !HasValidationErrors;

        public bool IsConvertable
        {
            get => isConvertable;
            set => Set(ref isConvertable, value);
        }

        public bool IsConverting
        {
            get => isConverting;
            set => Set(ref isConverting, value);
        }

        public bool HasValidationErrors
        {
            get => hasValidationErrors;
            set => Set(ref hasValidationErrors, value);
        }

        public bool IsNameChanging
        {
            get => isNameChanging;
            set => Set(ref isNameChanging, value);
        }

        public bool ShowPreview
        {
            get => showPreview;
            set => Set(ref showPreview, value);
        }

        public string PdfPath
        {
            get => pdfPath;
            set => Set(ref pdfPath, value);
        }

        public ConverterType ConverterType
        {
            get => converterType;
            set
            {
                Data.ToType = value;
                Set(ref converterType, value);
            }
        }

        public ConverterType[] ConverterTypes { get; } = new ConverterType[]
        {
            ConverterType.Latex, ConverterType.Pdf
        };

        public SearchViewModel Search { get; private set; }

        public ObservableCollection<WorkspaceItemViewModel> Children { get; private set; } = new ObservableCollection<WorkspaceItemViewModel>();
        public ICommand SaveCommand => new ActionCommand(Save);
        public ICommand CloseCommand => new ActionCommand(Close);
        public ICommand ConvertCommand => new ActionCommand(Convert);
        public ICommand StopConvertCommand => new ActionCommand(StopConvert);
        public ICommand HidePreviewCommand => new ActionCommand(() => ShowPreview = false);
        public ICommand ShowPreviewCommand => new ActionCommand(() => ShowPreview = true);
        [ChangeListener(nameof(PdfPath))]
        public ICommand PrintCommand => string.IsNullOrEmpty(PdfPath) ? null : new PrintCommand(Name, PdfPath);
        public ICommand RenameCommand => new ActionCommand(() => IsNameChanging = true);
        public ICommand CancelNameChangeCommand => new ActionCommand(CancelNameChange);
        public ICommand NameChangeEndCommand => new ActionCommand(NameChangeEnd);
        public ICommand SelectItemCommand => new ActionCommand(SelectItem);
        public ICommand DeleteCommand => new ActionCommand(Delete);
        [ChangeListener(nameof(FullName))]
        public ICommand OpenInExplorerCommand => new OpenExplorerCommand(FullName);
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
                {
                    outline.Exchange(value);
                }
                else
                {
                    outline = value;
                }
                SendPropertyUpdate();
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

        IEnumerable<WorkspaceItemViewModel> IExpandable<WorkspaceItemViewModel>.Children => Children;

        private string pdfPath;
        private bool isCompiled;
        private bool isExpanded = false;
        private bool isConvertable;
        private bool isConverting;
        private bool hasValidationErrors;
        private bool hasChanged;
        private bool isNameChanging;
        private bool showPreview;
        private string tempName;
        private object view;
        private int? wordCount;
        private OutlineViewModel outline;
        private CancelToken converterToken;
        private ConverterType converterType;

        public WorkspaceItemViewModel(WorkspaceViewModel workspaceViewModel, WorkspaceItemViewModel parent, IWorkspaceItem workspaceItem) 
            : base(workspaceItem ?? throw new ArgumentNullException(nameof(workspaceItem)))
        {
            Workspace = workspaceViewModel ?? throw new ArgumentNullException(nameof(workspaceViewModel));
            Parent = parent;

            IsConvertable = IsPlainText(workspaceItem);

            foreach (var child in workspaceItem.Children)
            {
                Children.Add(new WorkspaceItemViewModel(workspaceViewModel, this, child));
            }
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

            if (!Data.Children.Contains(child.Data))
            {
                throw new InvalidDataException("Underlying WorkspaceItem has to contain the specified child");
            }
            if (Children.Any(e => e.FullName == child.FullName))
            {
                return;
            }
            Children.Add(child);
        }

        public void StopConvert()
        {
            if (!IsConverting || converterToken is null)
                return;

            converterToken.Cancel();
            Messages.Warning(Language.Current.Get("Workspace.Compilation.Cancelled"));
        }

        public async Task Convert()
        {
            if (!CanConvert)
                return;
            
            converterToken = new CancelToken();
            AppViewModel.Instance.Messages.Working(Language.Current.Get("Workspace.Compilation.Running"));
            IsConverting = true;
            await Save();
            var watch = Stopwatch.StartNew();
            try
            {
                PdfPath = string.Empty;
                PdfPath = await Data.Convert(converterToken);
                IsCompiled = true;
                ShowPreview = true;
                watch.Stop();
                Messages.Success(Language.Current.Get("Workspace.Compilation.Success", watch.Elapsed.Seconds, watch.Elapsed.Milliseconds));
            }
            catch (Exception e)
            {
                if (!converterToken.IsCanceled)
                {
                    Messages.Error(ErrorUtility.GetErrorMessage(e));
                }
                IsCompiled = false;
            }
            watch.Stop();

            IsConverting = false;
        }

        public async Task Delete()
        {
            var lang = Language.Current;
            if (!IsNameChanging && await ShowMessageAsync(lang.Get("Workspace.Delete.File.Title"), lang.Get("Workspace.Delete.File.Text", Name), MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                await Remove();
            }
        }

        public async Task Remove()
        {
            await RemoveFromOpenItemsAsync();
            await Data.Delete();
            if (Parent != null)
            {
                await Dispatcher.InvokeAsync(() => Parent.Children.Remove(this));
            }
            string hash = IOUtility.GetHashFile(Data.FileInfo.FileSystem.Directory, Data.FullName);
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
            foreach (var child in Children)
            {
                child.RemoveFromOpenItems();
            }
        }

        private async Task RemoveFromOpenItemsAsync()
        {
            await Dispatcher.InvokeAsync(RemoveFromOpenItems);
        }

        public async Task Rename(string newName)
        {
            try
            {
                var oldHash = IOUtility.GetHashFile(Data.FileInfo.FileSystem.Directory, FullName);
                await Data.Rename(newName);
                try
                {
                    if (File.Exists(oldHash))
                    {
                        var newHash = IOUtility.GetHashFile(Data.FileInfo.FileSystem.Directory, FullName);
                        File.Move(oldHash, newHash);
                        PdfPath = newHash;
                    }
                }
                catch
                {
                    // Some IO error, this is fine
                }
            }
            catch
            {
                Trace.WriteLine("Could not rename file to: " + newName);
            }
            SendPropertyUpdate(nameof(Name));
        }

        public async Task Save()
        {
            if (view is IEditor editorWrapper)
            {
                IsCompiled = false;
                string text = await Dispatcher.InvokeAsync(() => editorWrapper.Editor.Text);
                HasChanged = false;
                await Data.Save(text);
            }
        }

        public async Task Close()
        {
            if (Workspace.OpenItems.Contains(this))
            {
                if (HasChanged)
                {
                    var lang = Language.Current;
                     switch (await ShowMessageAsync(lang.Get("Workspace.Save.File.Title"), lang.Get("Workspace.Save.File.Text"), MessageBoxButton.YesNoCancel))
                     {
                        case MessageBoxResult.Yes:
                            await Save();
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

                await Dispatcher.InvokeAsync(() => Workspace.OpenItems.Remove(this));

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

        private async Task NameChangeEnd()
        {
            await Rename(tempName);
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
                    return ShowDefaultEditor();
                case WorkspaceItemType.Image:
                    return ShowImage();
                default:
                    return null;
            }
        }

        private DocumentViewer ShowPdf()
        {
            var docViewer = new DocumentViewer();

            Task.Run(async () =>
            {
                try
                {
                    var bytes = await Data.Read();
                    var temp = IOUtility.GetTempFile(Data.FileInfo.FileSystem.Directory);
                    await IOUtility.WriteAllBytes(temp, Data.FileInfo.FileSystem.File, bytes);
                    PdfPath = temp;
                }
                catch (Exception e)
                {
                    Workspace.Messages.Error(ErrorUtility.GetErrorMessage(e));
                }
            });
            
            return docViewer;
        }

        private IEditor ShowMdEditorAndPdf()
        {
            var editorAndViewer = new EditorAndViewer();
            var temp = IOUtility.GetHashFile(Data.FileInfo.FileSystem.Directory, Data.FullName);
            if (File.Exists(temp))
            {
                PdfPath = temp;
                ShowPreview = true;
            }
            Task.Run(async () =>
            {
                string allText = ReadText(await Data.Read());
                await Dispatcher.InvokeAsync(() => editorAndViewer.Editor.Text = allText);
            });
            
            return editorAndViewer;
        }

        private IEditor ShowDefaultEditor()
        {
            var editor = new DefaultEditor();
            Task.Run(async () =>
            {
                string allText = ReadText(await Data.Read());
                await Dispatcher.InvokeAsync(() => editor.Editor.Text = allText);
            });
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
                Task.Run(async () =>
                {
                    using (var ms = new MemoryStream())
                    {

                        var bytes = await Data.Read();
                        ms.Write(bytes, 0, bytes.Length);
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = ms;
                        bitmap.EndInit();
                        bitmap.Freeze();
                        await Dispatcher.BeginInvoke((Action)(() =>
                        {
                            image.Source = bitmap;
                        }));
                        
                    }
                });
            }
            catch
            {
                Messages.Error(Language.Current.Get("Workspace.Image.Failed"));
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