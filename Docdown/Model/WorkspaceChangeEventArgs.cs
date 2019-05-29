using System;

namespace Docdown.Model
{
    [Flags]
    public enum WorkspaceChange : byte
    {
        Created = 1,
        Deleted = 2,
        Changed = 4,
        Renamed = 8,
        All = 15
    }

    public delegate void WorkspaceChangeEventHandler(IWorkspace workspace, WorkspaceChangeEventArgs args);

    public class WorkspaceChangeEventArgs : EventArgs
    {
        public IWorkspaceItem Item { get; }

        public WorkspaceChange Change { get; }

        public WorkspaceChangeEventArgs(IWorkspaceItem item, WorkspaceChange change)
        {
            Item = item;
            Change = change;
        }
    }
}
