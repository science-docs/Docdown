using Docdown.Util;
using Docdown.ViewModel.Commands;
using System;
using System.Collections.Generic;
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
        
        [ChangeListener(nameof(Search))]
        public IEnumerable<Explorer> Children => SearchChildren();

        public Explorer Parent { get; }

        public ICommand ClearSearchCommand => new ActionCommand(() => SetSearch(string.Empty));

        private string search = string.Empty;
        private readonly Action<string> debouncedSearch;

        public Explorer(WorkspaceViewModel workspace)
        {
            Workspace = workspace;
            Items.Add(new Explorer(this, workspace.Item));
            debouncedSearch = UIUtility.Debounce<string>(SetSearch, 500);
        }

        public Explorer(Explorer parent, WorkspaceItemViewModel workspaceItem)
        {
            Parent = parent;
            WorkspaceItem = workspaceItem;
            Workspace = WorkspaceItem.Workspace;
            foreach (var child in WorkspaceItem.Children)
            {
                Items.Add(new Explorer(this, child));
            }
        }

        private void SetSearch(string value)
        {
            search = value;
            SendPropertyUpdate(nameof(Search));
            Items.ForEach(e => e.Search = search);
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
