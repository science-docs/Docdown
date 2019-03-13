using Docdown.ViewModel.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                Set(ref search, value);
                Items.ForEach(e => e.Search = value);
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

        public ICommand ClearSearchCommand => new ActionCommand(() => Search = string.Empty);

        private string search = string.Empty;

        public Explorer(WorkspaceViewModel workspace)
        {
            Workspace = workspace;
            Items.Add(new Explorer(workspace.Item));
        }

        public Explorer(WorkspaceItemViewModel workspaceItem)
        {
            WorkspaceItem = workspaceItem;
            Workspace = WorkspaceItem.Workspace;
            foreach (var child in WorkspaceItem.Children)
            {
                Items.Add(new Explorer(child));
            }
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
