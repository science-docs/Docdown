﻿using Docdown.Model;
using Docdown.Net;
using Docdown.Util;
using Docdown.ViewModel.Commands;
using Docdown.Windows;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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

        [ChangeListener(nameof(Data))]
        public WorkspaceItemViewModel SelectedPreview
        {
            get
            {
                return selectedPreview;
            }
            set
            {
                if (value != null &&
                    value.IsFile &&
                    !OpenItems.Contains(value))
                {
                    OpenItems.Add(value);
                }

                selectedPreview = value;
                var item = value?.Data;

                if (item is null || !item.IsDirectory)
                {
                    SendPropertyUpdate();
                }
            }
        }

        public WorkspaceItemViewModel PreSelectedItem { get; set; }

        [ChangeListener(nameof(Data))]
        public IEnumerable<WorkspaceItemViewModel> Children => Item == null ? Array.Empty<WorkspaceItemViewModel>() : new[] { Item };

        

        [ChangeListener(nameof(SelectedItem))]
        public ConverterType FromType => Data?.FromType ?? ConverterType.Markdown;

        public ConverterType ToType
        {
            get => Data?.ToType ?? ConverterType.Pdf;
            set
            {
                Data.ToType = value;
                SendPropertyUpdate();
            }
        }

        public WizardViewModel Wizard { get; }
        public Explorer Explorer
        {
            get => explorer;
            private set => Set(ref explorer, value);
        }

        public string PdfPath
        {
            get => pdfPath;
            set => Set(ref pdfPath, value);
        }

        [ChangeListener(nameof(IsConverting))]
        public bool CanConvert => !IsConverting;

        public bool IsConverting
        {
            get => isConverting;
            set => Set(ref isConverting, value);
        }

        public bool IsCompiled
        {
            get => isCompiled;
            set => Set(ref isCompiled, value);
        }

        [ChangeListener(nameof(PdfPath))]
        public ICommand PrintCommand => string.IsNullOrEmpty(PdfPath) ? null : new PrintCommand(Item.Name, PdfPath);
        public ICommand SaveSelectedItemCommand => new ActionCommand(SaveSelectedItem);
        public ICommand SaveAllItemsCommand => new ActionCommand(SaveAllItems);
        public ICommand CloseAllItemsCommand => new ActionCommand(CloseAll);
        public ICommand OpenWizardCommand => new OpenWindowCommand<WizardWindow>(Wizard);
        public ICommand ChangeSelectedItemNameCommand => new ActionCommand(ChangeSelectedItemName);
        public ICommand DeleteSelectedItemCommand => new ActionCommand(DeleteSelectedItem);
        public ICommand ImportCommand => new ImportCommand(this, ConverterType.Markdown);
        public ICommand ConvertCommand => new ActionCommand(Convert);
        public ICommand StopConvertCommand => new ActionCommand(StopConvert);

        private Explorer explorer;
        private WorkspaceItemViewModel item;
        private WorkspaceItemViewModel selectedItem;
        private WorkspaceItemViewModel selectedPreview;
        private WorkspaceItemViewModel pdfPreview;
        private CancelToken converterToken;
        private string pdfPath;
        private bool isConverting;
        private bool isCompiled;

        public WorkspaceViewModel() : base(null)
        {
            Wizard = new WizardViewModel(this);
            Explorer = new Explorer(this);
        }

        public async Task Convert()
        {
            if (!CanConvert)
                return;

            converterToken = new CancelToken();
            Messages.Working(Language.Current.Get("Workspace.Compilation.Running"));
            IsConverting = true;
            await SaveAllItems();
            var watch = Stopwatch.StartNew();
            try
            {
                PdfPath = string.Empty;
                PdfPath = await Data.Convert(converterToken);
                IsCompiled = true;
                watch.Stop();
                var mil = watch.ElapsedMilliseconds.ToString("000");
                Messages.Success(Language.Current.Get("Workspace.Compilation.Success", watch.Elapsed.Seconds, mil));
            }
            catch (Exception e)
            {
                if (!converterToken.IsCanceled)
                {
                    Messages.Error(await ErrorUtility.GetErrorMessage(e, Dispatcher));
                }
                IsCompiled = false;
            }
            watch.Stop();

            IsConverting = false;
        }

        public void StopConvert()
        {
            if (!IsConverting || converterToken is null)
                return;

            converterToken.Cancel();
            Messages.Warning(Language.Current.Get("Workspace.Compilation.Cancelled"));
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

        public async Task SaveAllItems()
        {
            foreach (var item in OpenItems.Where(e => e.HasChanged))
            {
                await item.Save();
            }
        }

        public async Task OpenItem(string fullPath)
        {
            var item = SearchForSelectedItem(Item, fullPath);
            if (item != null)
            {
                await AddOpenItemAsync(item);
                SelectedItem = item;
            }
        }

        public async Task OnClosing(CancelEventArgs args)
        {
            if (Data == null)
                return;

            if (OpenItems.Any(e => e.HasChanged))
            {
                var lang = Language.Current;
                switch (await ShowMessageAsync(lang.Get("Workspace.Save.Files.Title"), lang.Get("Workspace.Save.Files.Text"), MessageBoxButton.YesNoCancel))
                {
                    case MessageBoxResult.Yes:
                        foreach (var openItem in OpenItems.Where(e => e.HasChanged))
                        {
                            await openItem.Save();
                        }
                        break;
                    case MessageBoxResult.None:
                    case MessageBoxResult.Cancel:
                        args.Cancel = true;
                        break;
                }
            }
            if (!args.Cancel)
            {
                await FillSettings();
                Data.Settings.Save();
            }
        }

        private async Task FillSettings()
        {
            var s = Data.Settings;
            s.Items.Clear();

            await TraverseExplorerSettings(s, Explorer.Items.First());
        }

        private async Task TraverseExplorerSettings(WorkspaceSettings settings, Explorer explorer)
        {
            var explorerItem = explorer.WorkspaceItem;
            WorkspaceItemPersistance item = new WorkspaceItemPersistance
            {
                Path = explorerItem.FullName,
                IsExcluded = explorerItem.IsExcluded,
                IsExpanded = explorerItem.IsExpanded
            };
            if (OpenItems.Contains(explorerItem))
            {
                item.IsSelected = explorerItem == SelectedItem;
                //if (explorerItem.Editor != null)
                //{
                //    item.ScrollOffset = await CalculateCurrentMidOffset(explorerItem.Editor.TextEditor.TextArea.TextView);
                //}
            }

            if (item.IsNecessary())
            {
                settings.Items.Add(item);
            }

            foreach (var child in explorer.Items)
            {
                await TraverseExplorerSettings(settings, child);
            }
        }

        private async Task<int> CalculateCurrentMidOffset(TextView view)
        {
            await Dispatcher.InvokeAsync(() => view.EnsureVisualLines());
            var lines = view.VisualLines;
            if (lines.Count > 0)
            {
                var middleLine = lines[lines.Count / 2];
                var index = middleLine?.FirstDocumentLine?.Offset ?? 0;
                return index;
            }
            return 0;
        }

        [ChangeListener(nameof(Data))]
        private void DataChanged()
        {
            SelectedItem = null;
            OpenItems.Clear();
            RefreshExplorer();
            RestoreSettings();

            LoadPreview();

            Data.WorkspaceChanged += OnWorkspaceChanged;
        }

        private void LoadPreview()
        {
            if (Item != null)
            {
                var temp = IOUtility.GetHashFile(Data.FileSystem.Directory, Item.FullName);
                if (Data.FileSystem.File.Exists(temp))
                {
                    PdfPath = temp;
                }
            }
        }

        private void RestoreSettings()
        {
            RestoreExplorerSettings(Data.Settings, Explorer.Items.First());
            if (SelectedItem == null && OpenItems.Count > 0)
            {
                SelectedItem = OpenItems.First();
            }
        }

        private void RestoreExplorerSettings(WorkspaceSettings settings, Explorer explorer)
        {
            var workspaceItem = explorer.WorkspaceItem;
            foreach (var item in settings.Items)
            {
                if (item.Path == workspaceItem.FullName)
                {
                    workspaceItem.IsExcluded = item.IsExcluded;

                    if (workspaceItem.IsFile)
                    {
                        AddOpenItem(workspaceItem);
                        if (item.IsSelected)
                        {
                            SelectedItem = workspaceItem;
                        }
                        //if (workspaceItem.Editor != null)
                        //{
                        //    workspaceItem.Editor.TextEditor.ScrollTo(item.ScrollOffset);
                        //}
                    }
                    else if (workspaceItem.IsDirectory)
                    {
                        explorer.IsExpanded = item.IsExpanded;
                    }
                }
            }

            foreach (var child in explorer.Children)
            {
                RestoreExplorerSettings(settings, child);
            }
        }

        private async void OnWorkspaceChanged(IWorkspace workspace, IEnumerable<WorkspaceChangeEventArgs> args)
        {
            if (workspace != Data)
            {
                return;
            }

            foreach (var e in args)
            {
                switch (e.Change)
                {
                    case WorkspaceChange.Deleted:
                        var item = SearchForSelectedItem(Item, e.Item);
                        // Item may have already been deleted, so nothing will be found
                        if (item != null)
                        {
                            await item.Remove(false);

                            if (item == Item)
                            {
                                Explorer = null;
                            }
                        }
                        break;
                    case WorkspaceChange.Created:
                        var parentItem = e.Item.Parent;
                        item = SearchForSelectedItem(Item, parentItem);
                        if (item != null)
                        {
                            item.AddChild(e.Item);
                        }
                        break;
                    case WorkspaceChange.Renamed:
                        item = SearchForSelectedItem(Item, e.Item);
                        item?.SendPropertyUpdate(nameof(WorkspaceItemViewModel.Name));
                        break;
                    case WorkspaceChange.Changed:
                        if (e.Item.Type == WorkspaceItemType.Bibliography)
                        {
                            Data.Bibliography.Parse(e.Item);
                        }
                        //item = SearchForSelectedItem(Item, e.Item);
                        break;
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
            if (item == null)
                return null;

            return SearchForSelectedItem(vm, item.FullName);
        }

        private WorkspaceItemViewModel SearchForSelectedItem(WorkspaceItemViewModel vm, string fullPath)
        {
            return vm.FirstOrDefault(e => e.Children, e => e.FullName == fullPath);
        }

        public void UpdateIcons(IEnumerable<WorkspaceItemViewModel> items)
        {
            foreach (var item in items)
            {
                item.SendPropertyUpdate(nameof(WorkspaceItemViewModel.IconName));
                UpdateIcons(item.Children);
            }
        }

        private async Task AddOpenItemAsync(WorkspaceItemViewModel item)
        {
            await Dispatcher.InvokeAsync(() => AddOpenItem(item));
        }

        private void AddOpenItem(WorkspaceItemViewModel item)
        {
            if (!OpenItems.Contains(item))
            {
                OpenItems.Add(item);
            }
        }

        [ChangeListener(nameof(PdfPath))]
        private void PdfPathChanged()
        {
            if (pdfPreview == null)
            {
                if (string.IsNullOrEmpty(PdfPath))
                    return;

                var newItem = new PreviewWorkspaceItem
                {
                    Type = WorkspaceItemType.Pdf,
                    FullName = PdfPath
                };

                pdfPreview = new WorkspaceItemViewModel(this, null, newItem)
                {
                    Name = Item.Name + " [Preview]",
                    PdfPath = PdfPath
                };
            }
            else
            {
                pdfPreview.PdfPath = PdfPath;
            }

            Dispatcher.InvokeAsync(() =>
            {
                SelectedPreview = pdfPreview;
            });
        }
    }
}
