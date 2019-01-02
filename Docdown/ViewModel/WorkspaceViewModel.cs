using Docdown.Model;
using Docdown.Util;
using Docdown.ViewModel.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Docdown.ViewModel
{
    public class WorkspaceViewModel : ObservableObject<Workspace>
    {
        [ChangeListener(nameof(Data))]
        public WorkspaceItemViewModel Item
        {
            get
            {
                if (item == null || item.Data != Data.Item)
                {
                    item = new WorkspaceItemViewModel(this, Data.Item);
                }
                return item;
            }
        }

        [ChangeListener(nameof(Data))]
        public ObservableCollection<WorkspaceItemViewModel> OpenItems
        {
            get; set;
        } = new ObservableCollection<WorkspaceItemViewModel>();


        [ChangeListener(nameof(Data))]
        public WorkspaceItemViewModel SelectedItem
        {
            get
            {
                if (selectedItem == null && Data.SelectedItem != null)
                {
                    selectedItem = new WorkspaceItemViewModel(this, Data.SelectedItem);
                }
                return selectedItem;
            }
            set
            {
                if (value != null && !value.Data.IsDirectory() && !OpenItems.Contains(value))
                {
                    OpenItems.Add(value);
                }

                selectedItem = value;
                Data.SelectedItem = value?.Data;
                var item = Data.SelectedItem;

                if (item == null || !item.IsDirectory())
                {
                    SendPropertyUpdate();
                }
            }
        }

        [ChangeListener(nameof(SelectedItem))]
        public string SelectedItemName => Data?.SelectedItem?.FileSystemInfo?.Name ?? "";

        [ChangeListener(nameof(Data))]
        public IEnumerable<WorkspaceItemViewModel> Children => Item.Children;

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

        public string ErrorMessage
        {
            get => errorMessage;
            set => Set(ref errorMessage, value);
        }

        public SettingsViewModel Settings { get; }

        public ICommand SaveSelectedItemCommand => new ActionCommand(SaveSelectedItem);
        public ICommand SaveAllItemsCommand => new ActionCommand(SaveAllItems);
        public ICommand SearchWorkspaceCommand => new SearchWorkspaceCommand(Settings.WorkspacePath, ChangeWorkspace);
        
        private string errorMessage;
        private WorkspaceItemViewModel item;
        private WorkspaceItemViewModel selectedItem;
        
        public WorkspaceViewModel(Workspace workspace) : base(workspace)
        {
            Settings = new SettingsViewModel(ChangeWorkspace);
            Template = Settings.Template;
        }
        
        private void SaveSelectedItem()
        {
            SelectedItem?.Save();
        }

        private void SaveAllItems()
        {
            foreach (var item in OpenItems)
            {
                item.Save();
            }
        }

        private void ChangeWorkspace(string newWorkspace)
        {
            if (newWorkspace == null)
                return;

            Settings.WorkspacePath = newWorkspace;
            Settings.Save();
            Data = new Workspace(newWorkspace)
            {
                ToType = ConverterType.Pdf
            };
        }

        [ChangeListener(nameof(Data))]
        private void DataChanged()
        {
            SelectedItem = null;
            OpenItems.Clear();
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
