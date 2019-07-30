using Docdown.Model;
using Docdown.Util;
using Docdown.ViewModel.Commands;
using Docdown.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Docdown.ViewModel
{
    public class WorkspaceViewModel : ObservableObject<IWorkspace>
    {
        [ChangeListener(nameof(Data))]
        public WorkspaceItemViewModel Item
        {
            get
            {
                if (Data != null && (item is null || item.Data != Data.Item))
                {
                    item = new WorkspaceItemViewModel(this, null, Data.Item)
                    {
                        IsExpanded = true
                    };
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
                if (selectedItem is null && Data?.SelectedItem != null)
                {
                    selectedItem = SearchForSelectedItem(Item, Data.SelectedItem);
                }
                return selectedItem;
            }
            set
            {
                if (value != null && 
                    value.IsFile && 
                    !OpenItems.Contains(value))
                {
                    OpenItems.Add(value);
                }

                selectedItem = value;
                var item = value?.Data;
                if (Data != null)
                {
                    Data.SelectedItem = item;
                }

                if (item is null || !item.IsDirectory)
                {
                    SendPropertyUpdate();
                }
            }
        }

        public WorkspaceItemViewModel PreSelectedItem { get; set; }

        [ChangeListener(nameof(Data))]
        public IEnumerable<WorkspaceItemViewModel> Children => Item == null ? new WorkspaceItemViewModel[0] : new[] { Item };

        

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


        /// <summary>
        /// Indicates whether the workspace is currently being changed programmatically in order to ignore the change message.
        /// </summary>
        public bool IgnoreChange
        {
            // TODO: Fix this
            get; set;
            //get => Data.IgnoreChange;
            //set => Data.IgnoreChange = value;
        }

        public WizardViewModel Wizard { get; }
        public Explorer Explorer
        {
            get => explorer;
            private set => Set(ref explorer, value);
        }

        public ICommand SaveSelectedItemCommand => new ActionCommand(SaveSelectedItem);
        public ICommand SaveAllItemsCommand => new ActionCommand(SaveAllItems);
        public ICommand CloseAllItemsCommand => new ActionCommand(CloseAll);
        public ICommand OpenWizardCommand => new OpenWindowCommand<WizardWindow>(Wizard);
        public ICommand ChangeSelectedItemNameCommand => new ActionCommand(ChangeSelectedItemName);
        public ICommand DeleteSelectedItemCommand => new ActionCommand(DeleteSelectedItem);
        public ICommand ImportCommand => new ImportCommand(this, ConverterType.Markdown);
        
        private Explorer explorer;
        private WorkspaceItemViewModel item;
        private WorkspaceItemViewModel selectedItem;
        
        public WorkspaceViewModel() : base(null)
        {
            Wizard = new WizardViewModel(this);
            Explorer = new Explorer(this);
        }

        public void CloseAll()
        {
            SelectedItem = null;
            OpenItems.Clear();
        }
        
        private async Task SaveSelectedItem()
        {
            await SelectedItem?.Save();
        }

        private async Task SaveAllItems()
        {
            foreach (var item in OpenItems)
            {
                await item.Save();
            }
        }

        public void OpenItem(string fullPath)
        {
            var item = SearchForSelectedItem(Item, fullPath);
            if (item != null)
            {
                OpenItems.Add(item);
                SelectedItem = item;
            }
            else
            {
                var parent = Path.GetDirectoryName(fullPath);
                Data = WorkspaceProvider.Create(new Uri(parent));
                Data.ToType = ConverterType.Pdf;
                OpenItem(fullPath);
            }
        }

        public async Task OnClosing(CancelEventArgs args)
        {
            if (OpenItems.Any(e => e.HasChanged))
            {
                var lang = Language.Current;
                switch (await ShowMessageAsync(lang.Get("Workspace.Save.Files.Title"), lang.Get("Workspace.Save.Files.Text"), MessageBoxButton.YesNoCancel))
                {
                    case MessageBoxResult.Yes:
                        foreach (var openItem in OpenItems)
                        {
                            if (openItem.HasChanged)
                            {
                                await openItem.Save();
                            }
                        }
                        break;
                    case MessageBoxResult.None:
                    case MessageBoxResult.Cancel:
                        args.Cancel = true;
                        break;
                }
            }
        }

        [ChangeListener(nameof(Data))]
        private void DataChanged()
        {
            SelectedItem = null;
            OpenItems.Clear();
            RefreshExplorer();
        }

        private void OnWorkspaceChanged(object sender, EventArgs args)
        {
            // in order to ignore multiple messages, simply ignore changes until the message has been answered
            IgnoreChange = true;
            var result = ShowMessage(
                "Workspace changed", 
                "Your workspace was changed externally. Do you want to reload your workspace?", 
                MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                var openItems = OpenItems.ToArray();
                var selectedItemName = SelectedItem?.RelativeName;
                Data = WorkspaceProvider.Create(new Uri(Data.Item.FullName));
                Data.ToType = ConverterType.Pdf;
                RestoreWorkspace(Item, openItems, selectedItemName);
            }
            IgnoreChange = false;
        }

        private void RestoreWorkspace(WorkspaceItemViewModel item, IEnumerable<WorkspaceItemViewModel> openItems, string selectedItemName)
        {
            foreach (var child in item.Children)
            {
                if (child.IsDirectory)
                {
                    RestoreWorkspace(child, openItems, null);
                }
                else if (openItems.Any(e => e.RelativeName == child.RelativeName))
                {
                    OpenItems.Add(child);
                }
            }
            Children.Restore(openItems);
            if (selectedItemName != null)
            {
                foreach (var i in OpenItems)
                {
                    if (i.RelativeName == selectedItemName)
                    {
                        SelectedItem = i;
                        break;
                    }
                }
            }
        }

        private void ChangeSelectedItemName()
        {
            if (PreSelectedItem != null)
            {
                PreSelectedItem.IsNameChanging = true;
            }
        }

        private async Task DeleteSelectedItem()
        {
            if (PreSelectedItem != null)
            {
                await PreSelectedItem.Delete();
            }
        }

        public void RefreshExplorer()
        {
            Explorer = new Explorer(this);
        }

        private WorkspaceItemViewModel SearchForSelectedItem(WorkspaceItemViewModel vm, IWorkspaceItem item)
        {
            if (vm.Data == item)
            {
                return vm;
            }
            else
            {
                foreach (var child in vm.Children)
                {
                    var found = SearchForSelectedItem(child, item);
                    if (found != null)
                    {
                        return found;
                    }
                }
                return null;
            }
        }

        private WorkspaceItemViewModel SearchForSelectedItem(WorkspaceItemViewModel vm, string fullPath)
        {
            if (vm.RelativeName == fullPath)
            {
                return vm;
            }
            else
            {
                foreach (var child in vm.Children)
                {
                    var found = SearchForSelectedItem(child, fullPath);
                    if (found != null)
                    {
                        return found;
                    }
                }
                return null;
            }
        }

        public void UpdateIcons(IEnumerable<WorkspaceItemViewModel> items)
        {
            foreach (var item in items)
            {
                item.SendPropertyUpdate(nameof(WorkspaceItemViewModel.IconName));
                UpdateIcons(item.Children);
            }
        }
    }
}