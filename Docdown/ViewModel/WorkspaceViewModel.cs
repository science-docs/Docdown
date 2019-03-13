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
using System.Windows;
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
                if (item is null || item.Data != Data.Item)
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
                if (selectedItem is null && Data.SelectedItem != null)
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
                var item = Data.SelectedItem = value?.Data;

                if (item is null || !item.IsDirectory())
                {
                    SendPropertyUpdate();
                }
            }
        }

        public WorkspaceItemViewModel PreSelectedItem { get; set; }

        [ChangeListener(nameof(Data))]
        public IEnumerable<WorkspaceItemViewModel> Children => new[] { Item };

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
            get => Data.IgnoreChange;
            set => Data.IgnoreChange = value;
        }

        public string ErrorMessage
        {
            get => errorMessage;
            set => Set(ref errorMessage, value);
        }

        public SettingsViewModel Settings { get; }
        public WizardViewModel Wizard { get; }
        public MessageQueue Messages { get; }
        public Explorer Explorer
        {
            get => explorer;
            private set => Set(ref explorer, value);
        }

        public ICommand SaveSelectedItemCommand => new ActionCommand(SaveSelectedItem);
        public ICommand SaveAllItemsCommand => new ActionCommand(SaveAllItems);
        public ICommand CloseAllItemsCommand => new ActionCommand(CloseAll);
        [ChangeListener(nameof(Data))]
        public ICommand SearchWorkspaceCommand => new SearchFolderCommand(Settings.WorkspacePath, "Select workspace", ChangeWorkspace);
        public ICommand OpenSettingsCommand => new OpenWindowCommand<SettingsWindow>(Settings);
        public ICommand OpenWizardCommand => new OpenWindowCommand<WizardWindow>(Wizard);
        public ICommand ChangeSelectedItemNameCommand => new ActionCommand(ChangeSelectedItemName);
        public ICommand DeleteSelectedItemCommand => new ActionCommand(DeleteSelectedItem);
        public ICommand ImportCommand => new ImportCommand(this, ConverterType.Markdown);

        private string errorMessage;
        private Explorer explorer;
        private WorkspaceItemViewModel item;
        private WorkspaceItemViewModel selectedItem;
        
        public WorkspaceViewModel(Workspace workspace) : base(workspace ?? throw new ArgumentNullException(nameof(workspace)))
        {
            Settings = new SettingsViewModel(this);
            Wizard = new WizardViewModel(this);
            Messages = new MessageQueue();
            workspace.WorkspaceChanged += OnWorkspaceChanged;
            Explorer = new Explorer(this);
        }

        public void CloseAll()
        {
            SelectedItem = null;
            OpenItems.Clear();
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

        public void ChangeWorkspace(string newWorkspace)
        {
            if (newWorkspace is null)
                return;

            Settings.WorkspacePath = newWorkspace;
            Settings.Save();
            Data = new Workspace(newWorkspace)
            {
                ToType = ConverterType.Pdf
            };
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
                Data = new Workspace(parent)
                {
                    ToType = Data.ToType
                };
                OpenItem(fullPath);
            }
        }

        public void OnClosing(CancelEventArgs args)
        {
            if (OpenItems.Any(e => e.HasChanged))
            {
                switch (ShowMessage("Save files", "Do you want to save your files before closing?", MessageBoxButton.YesNoCancel))
                {
                    case MessageBoxResult.Yes:
                        foreach (var openItem in OpenItems)
                        {
                            if (openItem.HasChanged)
                            {
                                openItem.Save();
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
                var selectedItemName = SelectedItem?.FullName;
                Data = new Workspace(Data.Item.FileSystemInfo.FullName)
                {
                    ToType = Data.ToType
                };
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
                else if (openItems.Any(e => e.FullName == child.FullName))
                {
                    OpenItems.Add(child);
                }
            }
            Children.Restore(openItems);
            if (selectedItemName != null)
            {
                foreach (var i in OpenItems)
                {
                    if (i.FullName == selectedItemName)
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

        private void DeleteSelectedItem()
        {
            if (PreSelectedItem != null)
            {
                PreSelectedItem.Delete();
            }
        }

        public void RefreshExplorer()
        {
            Explorer = new Explorer(this);
        }

        private WorkspaceItemViewModel SearchForSelectedItem(WorkspaceItemViewModel vm, WorkspaceItem item)
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
            if (vm.FullName == fullPath)
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
    }
}