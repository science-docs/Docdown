using Docdown.Model;
using Docdown.Util;
using Docdown.ViewModel.Commands;
using Docdown.Windows;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
                if (value != null && 
                    !value.Data.IsDirectory() && 
                    !OpenItems.Contains(value))
                {
                    OpenItems.Add(value);
                }

                selectedItem = value;
                var item = Data.SelectedItem = value?.Data;

                if (item == null || !item.IsDirectory())
                {
                    SendPropertyUpdate();
                }
            }
        }

        public WorkspaceItemViewModel SelectedWorkspaceItem { get; set; }

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

        public bool IsChanging
        {
            get => Data.IsChanging;
            set => Data.IsChanging = value;
        }

        public string ErrorMessage
        {
            get => errorMessage;
            set => Set(ref errorMessage, value);
        }

        public SettingsViewModel Settings { get; }

        public ICommand SaveSelectedItemCommand => new ActionCommand(SaveSelectedItem);
        public ICommand SaveAllItemsCommand => new ActionCommand(SaveAllItems);
        public ICommand CloseAllItemsCommand => new ActionCommand(CloseAll);
        [ChangeListener(nameof(Data))]
        public ICommand SearchWorkspaceCommand => new SearchFolderCommand(Settings.WorkspacePath, "Select workspace", ChangeWorkspace);
        public ICommand OpenSettingsCommand => new OpenWindowCommand<SettingsWindow>(Settings);
        public ICommand OpenWizardCommand => new OpenWindowCommand<WizardWindow>(this);
        public ICommand ChangeSelectedItemNameCommand => new ActionCommand(ChangeSelectedItemName);

        private string errorMessage;
        private WorkspaceItemViewModel item;
        private WorkspaceItemViewModel selectedItem;
        
        public WorkspaceViewModel(Workspace workspace) : base(workspace)
        {
            Settings = new SettingsViewModel(this);
            workspace.WorkspaceChanged += OnWorkspaceChanged;
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

        private async void OnWorkspaceChanged(object sender, EventArgs args)
        {
            var result = await ShowMessageAsync(
                "Workspace changed", 
                "Your workspace was changed externally. Do you want to reload your workspace?", 
                MessageDialogStyle.AffirmativeAndNegative);
            if (result == MessageDialogResult.Affirmative)
            {
                var openItems = OpenItems.ToArray();
                var selectedItemName = SelectedItem?.FullName;
                Data = new Workspace(Data.Item.FileSystemInfo.FullName)
                {
                    ToType = Data.ToType
                };
                RestoreWorkspace(Item, openItems, selectedItemName);
            }
        }

        private void RestoreWorkspace(WorkspaceItemViewModel item, IEnumerable<WorkspaceItemViewModel> openItems, string selectedItemName)
        {
            foreach (var child in item.Children)
            {
                if (child.Data.IsDirectory())
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
            if (SelectedWorkspaceItem != null)
            {
                SelectedWorkspaceItem.IsNameChanging = true;
            }
        }
    }
}