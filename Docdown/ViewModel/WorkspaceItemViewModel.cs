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
using System.Collections.ObjectModel;
using Docdown.ViewModel.Editing;

namespace Docdown.ViewModel
{
    public class WorkspaceItemViewModel : ObservableObject<IWorkspaceItem>, IExpandable<WorkspaceItemViewModel>
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
                    case WorkspaceItemType.Bibliography: return "CitationIcon";
                    default: return "DocumentIcon";
                }
            }
        }

        public bool IsCompiled
        {
            get => isCompiled;
            set => Set(ref isCompiled, value);
        }

        public bool IsExcluded
        {
            get => Data.IsExcluded;
            set
            {
                Data.IsExcluded = value;
                SendPropertyUpdate();
                foreach (var child in Children)
                {
                    child.SendPropertyUpdate();
                }
            }
        }

        [ChangeListener(nameof(IsExcluded))]
        public bool IsIncluded => !IsExcluded;

        [ChangeListener(nameof(IsExcluded))]
        public bool IsExcludedEffectively => Data.IsExcludedEffectively;

        public bool IsExpanded
        {
            get => isExpanded;
            set => Set(ref isExpanded, value);
        }

        public bool IsConvertable
        {
            get => isConvertable;
            set => Set(ref isConvertable, value);
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

        public EditorViewModel Editor { get; private set; }

        public ObservableCollection<WorkspaceItemViewModel> Children { get; private set; } = new ObservableCollection<WorkspaceItemViewModel>();
        public ICommand SaveCommand => new ActionCommand(Save);
        public ICommand CloseCommand => new ActionCommand(Close);
        public ICommand ExcludeCommand => new ActionCommand(() => IsExcluded = true);
        public ICommand IncludeCommand => new ActionCommand(() => IsExcluded = false);
        
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
                    view = BuildView(out var vm);
                    if (view is IEditor editor && view is FrameworkElement frameworkElement)
                    {
                        Search = new SearchViewModel(editor);
                        Editor = vm;
                        frameworkElement.DataContext = this;
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

        public bool IsPreview => Data is PreviewWorkspaceItem;

        public bool IsDirectory => Data.IsDirectory;
        public bool IsFile => Data.IsFile;

        public string PdfPath
        {
            get => pdfPath ?? Workspace.PdfPath;
            set => Set(ref pdfPath, value);
        }

        public WorkspaceViewModel Workspace { get; }
        public WorkspaceItemViewModel Parent { get; }

        IEnumerable<WorkspaceItemViewModel> IExpandable<WorkspaceItemViewModel>.Children => Children;

        private string pdfPath;
        private bool isCompiled;
        private bool isExpanded = false;
        private bool isConvertable;
        private bool hasValidationErrors;
        private bool hasChanged;
        private bool isNameChanging;
        private string tempName;
        private object view;
        private int? wordCount;
        private OutlineViewModel outline;
        private ConverterType converterType;

        // This is needed for the tab control to work correctly.
        // See issue #29 for an explanation.
        private int? originalHashcode;

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

            Workspace.PropertyChanged += PdfPathChanged;
        }

        private void PdfPathChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(WorkspaceViewModel.PdfPath))
            {
                SendPropertyUpdate(nameof(PdfPath));
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

       

        

        public async Task Delete()
        {
            var lang = Language.Current;
            if (!IsNameChanging && await ShowMessageAsync(lang.Get("Workspace.Delete.File.Title"), lang.Get("Workspace.Delete.File.Text", Name), MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                await Remove(true);
            }
        }

        public async Task Remove(bool delete)
        {
            await RemoveFromOpenItemsAsync();
            if (delete)
            {
                await Data.Delete();
            }
            else
            {
                Data.Parent.Children.Remove(Data);
            }
            if (Parent != null)
            {
                await Dispatcher.InvokeAsync(() => Parent.Children.Remove(this));
            }
            string hash = IOUtility.GetHashFile(Data.FileInfo.FileSystem.Directory, Data.FullName);
            if (Data.FileInfo.FileSystem.File.Exists(hash))
            {
                try
                {
                    Data.FileInfo.FileSystem.File.Delete(hash);
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
                    if (Data.FileInfo.FileSystem.File.Exists(oldHash))
                    {
                        var newHash = IOUtility.GetHashFile(Data.FileInfo.FileSystem.Directory, FullName);
                        Data.FileInfo.FileSystem.File.Move(oldHash, newHash);
                        Workspace.PdfPath = newHash;
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

        [ChangeListener(nameof(IsNameChanging))]
        private void IsNameChangingChanged()
        {
            if (IsNameChanging)
            {
                CancelChangeForItem(Workspace.Item, this);
            }

            void CancelChangeForItem(WorkspaceItemViewModel item, WorkspaceItemViewModel baseItem)
            {
                if (item == baseItem)
                    return;

                item.IsNameChanging = false;
                foreach (var child in item.Children)
                {
                    CancelChangeForItem(child, baseItem);
                }
            }
        }

        private async Task NameChangeEnd()
        {
            await Rename(tempName);
            CancelNameChange();
        }

        private object BuildView(out EditorViewModel editorViewModel)
        {
            editorViewModel = null;
            switch (Data?.Type)
            {
                case WorkspaceItemType.Pdf:
                    return ShowPdf();
                case WorkspaceItemType.Latex:
                case WorkspaceItemType.Markdown:
                    return ShowMdEditorAndPdf(out editorViewModel);
                case WorkspaceItemType.Bibliography:
                    return ShowBibEditor(out editorViewModel);
                case WorkspaceItemType.Text:
                    return ShowDefaultEditor(out editorViewModel);
                case WorkspaceItemType.Image:
                    return ShowImage();
                case WorkspaceItemType.Web:
                    return ShowBrowser();
                default:
                    return null;
            }
        }

        private DocumentViewer ShowPdf()
        {
            var docViewer = new DocumentViewer();
            PdfPath = Data.FullName;
            return docViewer;
        }

        private IEditor ShowMdEditorAndPdf(out EditorViewModel vm)
        {
            var editor = new EditorControl();
            //var temp = IOUtility.GetHashFile(Data.FileInfo.FileSystem.Directory, Data.Workspace.Item.FullName);
            //if (Data.FileInfo.FileSystem.File.Exists(temp))
            //{
            //    Workspace.PdfPath = temp;
            //    ShowPreview = true;
            //}
            vm = new MarkdownEditorViewModel(this);
            return editor;
        }

        private IEditor ShowBibEditor(out EditorViewModel vm)
        {
            var editor = new EditorControl();
            vm = new BibEditorViewModel(this);
            return editor;
        }

        private IEditor ShowDefaultEditor(out EditorViewModel vm)
        {
            var editor = new EditorControl();
            vm = new DefaultEditorViewModel(this);
            return editor;
        }

        private void SetContent(IEditor editor)
        {
            string allText = ReadText(Data.Read());
            editor.Editor.Text = allText;
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

                        var bytes = Data.Read();
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

        private WebViewer ShowBrowser()
        {
            if (Data is PreviewWorkspaceItem preview)
            {
                var viewer = new WebViewer();
                viewer.Navigate(preview.FullName);
                return viewer;
            }
            return null;
        }

        private void SelectItem()
        {
            Workspace.SelectedItem = this;
        }
        
        public override int GetHashCode()
        {
            if (originalHashcode == null)
            {
                originalHashcode = base.GetHashCode();
            }
            return originalHashcode.Value;
        }
    }
}
