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
    public class WorkspaceViewModel : ObservableObject<Workspace>
    {
        public WorkspaceItemViewModel Item
        {
            get => new WorkspaceItemViewModel(Data.Item);
        }
        public WorkspaceItemViewModel SelectedItem
        {
            get => new WorkspaceItemViewModel(Data.SelectedItem);
            set
            {
                Data.SelectedItem = value.Data;
                if (!Data.SelectedItem.IsDirectory())
                    SendPropertyUpdate();

                if (Data.SelectedItem.IsPlainText())
                {
                    CanConvert = true;
                    SendPropertyUpdate(nameof(SelectedItemText));
                }
                else
                {
                    CanConvert = false;
                }
            }
        }

        [ChangeListener(nameof(SelectedItem))]
        public string SelectedItemName => Data?.SelectedItem?.FileSystemInfo?.Name ?? "";

        [ChangeListener(nameof(SelectedItem))]
        public OutlineViewModel Outline
        {
            get => outline;
            set
            {
                if (outline != null && value != null)
                    outline.Exchange(value);
                outline = value;
                SendPropertyUpdate();
            }
        }

        public OutlineItemViewModel SelectedOutlineItem
        {
            get;
            set;
        }

        [ChangeListener(nameof(Data))]
        public IEnumerable<WorkspaceItemViewModel> Children
            => Data.Item.Children
                .OrderByDescending(e => e.IsDirectory())
                .Select(e => new WorkspaceItemViewModel(e));

        [ChangeListener(nameof(SelectedItem))]
        public ConverterType FromType => Data.FromType;

        public ConverterType ToType
        {
            get => Data.ToType;
            set
            {
                Data.ToType = value;
                SendPropertyUpdate();
            }
        }

        public string Template
        {
            get => Data.Template;
            set
            {
                Data.Template = value;
                Settings.Template = value;
                SendPropertyUpdate();
            }
        }
        
        public string SelectedItemText
        {
            get
            {
                var selectedItem = Data.SelectedItem;
                if (selectedItem != null)
                {
                    string fullName = selectedItem.FileSystemInfo.FullName;
                    if (!textCache.TryGetValue(fullName, out string cachedText)
                        && selectedItem.IsPlainText())
                    {
                        cachedText = File.ReadAllText(fullName);
                        textCache[fullName] = cachedText;
                    }
                    return cachedText;
                }
                return string.Empty;
            }
            set
            {
                var selectedItem = Data.SelectedItem;
                if (selectedItem != null)
                {
                    string fullName = selectedItem.FileSystemInfo.FullName;
                    textCache[fullName] = value;
                }
            }
        }

        public string SelectedPdfPath
        {
            get => pdfPath;
            set => Set(ref pdfPath, value);
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

        public SettingsViewModel Settings { get; }

        public ICommand SaveSelectedItemCommand => new ActionCommand(SaveSelectedItem);
        public ICommand SaveAllItemsCommand => new ActionCommand(SaveAllItems);
        public ICommand ConvertCommand => new ActionCommand(Convert);
        public ICommand SearchWorkspaceCommand => new SearchWorkspaceCommand(Settings.WorkspacePath, ChangeWorkspace);
        [ChangeListener(nameof(SelectedPdfPath))]
        public ICommand PrintCommand => new PrintCommand(SelectedItemName, SelectedPdfPath);

        private bool canConvert;
        private bool isConverting;
        private string pdfPath;
        private string errorMessage;
        private OutlineViewModel outline;
        private Dictionary<string, string> textCache = new Dictionary<string, string>();
        
        public WorkspaceViewModel(Workspace workspace) : base(workspace)
        {
            Settings = new SettingsViewModel(ChangeWorkspace);
            Template = Settings.Template;
        }
        
        private void SaveSelectedItem()
        {
            var selectedItem = Data.SelectedItem;
            if (selectedItem != null)
            {
                string fullName = selectedItem.FileSystemInfo.FullName;
                File.WriteAllText(fullName, textCache[fullName]);
            }
        }

        private void SaveAllItems()
        {
            foreach (var pair in textCache)
            {
                File.WriteAllText(pair.Key, pair.Value);
            }
        }

        private void ChangeWorkspace(string newWorkspace)
        {
            if (newWorkspace == null)
                return;

            Task.Run(() =>
            {
                Settings.WorkspacePath = newWorkspace;
                Settings.Save();
                Data = new Workspace(newWorkspace)
                {
                    ToType = ConverterType.Pdf
                };
            });
        }

        private void Convert()
        {
            IsConverting = true;
            Task.Run(() =>
            {
                try
                {
                    var path = Data.Convert();
                    SelectedPdfPath = path;
                    ErrorMessage = "";
                }
                catch (Exception e)
                {
                    ErrorMessage = ErrorUtility.GetErrorMessage(e);
                }
                
                IsConverting = false;
            });
        }

        //private OutlineViewModel BuildOutline()
        //{
        //    var text = SelectedItemText;
        //    if (text != null)
        //    {
        //        var ast = AbstractSyntaxTree.GenerateAbstractSyntaxTree(text);
        //        var header = AbstractSyntaxTree.EnumerateHeader(ast);

        //        var outline = new Outline(header);
        //        return new OutlineViewModel(outline);
        //    }
        //    else
        //    {
        //        return new OutlineViewModel(new Outline());
        //    }
        //}

        //private void BuildOutlineDiff(Outline newOutline)
        //{


        //    outline = newOutline;
        //}
    }
}
