using Docdown.Util;
using Docdown.ViewModel.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace Docdown.ViewModel
{
    public class Explorer : ObservableObject
    {
        public WorkspaceItemViewModel WorkspaceItem { get; }
        public WorkspaceViewModel Workspace { get; }

        public string Search
        {
            get => search;
            set
            {
                if (value is null)
                {
                    value = string.Empty;
                }
                search = value;
                if (WorkspaceItem == null)
                {
                    debouncedSearch(value);
                }
                else
                {
                    SetSearch(value);
                }
            }
        }

        public bool IsExpanded
        {
            get => WorkspaceItem.IsExpanded;
            set => WorkspaceItem.IsExpanded = value;
        }

        public List<Explorer> Items { get; } = new List<Explorer>();
        
        public ObservableCollection<Explorer> Children { get; } = new ObservableCollection<Explorer>();

        public Explorer Parent { get; }

        public ICommand ClearSearchCommand => new ActionCommand(() => SetSearch(string.Empty));

        private string search = string.Empty;
        private readonly Action<string> debouncedSearch;

        public Explorer(WorkspaceViewModel workspace)
        {
            Workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));
            if (workspace.Data != null)
            {
                Items.Add(new Explorer(this, workspace.Item));
                Children.Add(Items[0]);
            }
            debouncedSearch = UIUtility.Debounce<string>(SetSearch, 300);
        }

        private Explorer(Explorer parent, WorkspaceItemViewModel workspaceItem)
        {
            Parent = parent ?? throw new ArgumentNullException(nameof(parent));
            WorkspaceItem = workspaceItem ?? throw new ArgumentNullException(nameof(workspaceItem));
            Workspace = WorkspaceItem.Workspace;
            foreach (var child in WorkspaceItem.Children)
            {
                var explorer = new Explorer(this, child);
                Insert(explorer, true);
            }
            WorkspaceItem.Children.CollectionChanged += Children_CollectionChanged;
            WorkspaceItem.Changed(nameof(WorkspaceItemViewModel.Name), Rename);
        }

        private async void Rename()
        {
            await Dispatcher.InvokeAsync(() =>
            {
                Parent.Children.Remove(this);
                if (Contains(search))
                {
                    Parent.Insert(this);
                }
            });
        }

        private void Children_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems)
                {
                    var vmItem = item as WorkspaceItemViewModel;
                    var explorer = new Explorer(this, vmItem);
                    explorer.SetSearch(Search);
                    Insert(explorer);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in e.OldItems)
                {
                    var explorer = Items.Find(i => i.WorkspaceItem == item);
                    Items.Remove(explorer);
                    Children.Remove(explorer);
                }
            }
        }

        private void Insert(Explorer explorer, bool ignore = false)
        {
            Items.Add(explorer);
            if (ignore || explorer.Contains(Search))
            {
                bool isFile = explorer.WorkspaceItem.IsFile;
                bool same = explorer.WorkspaceItem.IsDirectory;
                bool first = true;
                for (int i = 0; i < Children.Count; i++)
                {
                    var child = Children[i];
                    if (isFile == child.WorkspaceItem.IsFile)
                    {
                        same = true;
                        if (string.Compare(explorer.WorkspaceItem.Name, child.WorkspaceItem.Name, true) < 0)
                        {
                            Children.Insert(i, explorer);
                            return;
                        }
                    }
                    else if (same && !first)
                    {
                        Children.Insert(i - 1, explorer);
                        return;
                    }
                    first = false;
                }
                Children.Add(explorer);
            }
        }

        public void Update()
        {

        }

        private void SetSearch(string value)
        {
            search = value;
            SendPropertyUpdate(nameof(Search));
            Items.ForEach(e => e.Search = search);
        }
        
        [ChangeListener(nameof(Search))]
        private void UpdateChildren()
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                Children.Clear();

                foreach (var child in SearchChildren()
                    .OrderByDescending(e => e.WorkspaceItem.IsDirectory)
                    .ThenBy(e => e.WorkspaceItem.Name))
                {
                    Children.Add(child);
                }
            }));
        }

        private IEnumerable<Explorer> SearchChildren()
        {
            if (string.IsNullOrEmpty(search))
            {
                foreach (var child in Items)
                {
                    yield return child;
                }
            }
            else
            {
                string lower = search.ToLower();
                foreach (var child in Items)
                {
                    if (child.Contains(lower))
                    {
                        yield return child;
                    }
                }
            }
        }

        private bool Contains(string searchValue)
        {
            if (WorkspaceItem.Name.ToLower().Contains(searchValue))
            {
                return true;
            }

            foreach (var child in Items)
            {
                if (child.Contains(searchValue))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
