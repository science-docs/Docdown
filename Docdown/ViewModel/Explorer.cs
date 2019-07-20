using Docdown.Util;
using Docdown.ViewModel.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
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
                Items.Add(explorer);
                Children.Add(explorer);
            }
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

                foreach (var child in SearchChildren())
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
