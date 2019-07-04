using Docdown.Util;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;

namespace Docdown.Model
{
    public class FileWorkspace : Workspace<FileWorkspaceItem>
    {
        public override FileWorkspaceItem Item { get; }
        public override FileWorkspaceItem SelectedItem { get; set; }
        public bool IgnoreChange
        {
            get => !watcher.EnableRaisingEvents;
            set => watcher.EnableRaisingEvents = !value;
        }

        public bool GitEnabled => Repository != null;
        public Repository Repository { get; }
        public override event WorkspaceChangeEventHandler WorkspaceChanged;

        private readonly FileSystemWatcher watcher;

        private readonly Action debounceUpdate;
        private readonly Stack<WorkspaceChangeEventArgs> stackedChanges;
        private readonly string path;

        public FileWorkspace(string path)
        {
            this.path = path;
            Repository = SetupGit(path);
            Item = new FileWorkspaceItem(path);

            stackedChanges = new Stack<WorkspaceChangeEventArgs>();
            watcher = new FileSystemWatcher(path);
            watcher.Changed += OnWorkspaceChanged;
            watcher.Created += OnWorkspaceChanged;
            watcher.Deleted += OnWorkspaceChanged;
            watcher.Renamed += OnWorkspaceChanged;
            watcher.NotifyFilter = NotifyFilters.LastWrite |
                                   NotifyFilters.DirectoryName |
                                   NotifyFilters.FileName;
            watcher.EnableRaisingEvents = true;
            watcher.IncludeSubdirectories = true;

            debounceUpdate = UIUtility.Debounce(() =>
            {
                while (stackedChanges.Count > 0)
                {
                    var change = stackedChanges.Pop();
                    WorkspaceChanged?.Invoke(this, change);
                }
            }, 1000);
        }

        private void OnWorkspaceChanged(object sender, FileSystemEventArgs e)
        {
            var change = FromFileSystemEvent(e);
            stackedChanges.Push(change);
            debounceUpdate();
        }

        private WorkspaceChangeEventArgs FromFileSystemEvent(FileSystemEventArgs e)
        {
            var full = e.FullPath;
            int diff = full.Length - path.Length;
            full = full.Substring(path.Length, diff);
            var item = FindRelativeItem(full);
            WorkspaceChange change = (WorkspaceChange)e.ChangeType;
            return new WorkspaceChangeEventArgs(item, change);
        }

        private Repository SetupGit(string path)
        {
            if (!Directory.Exists(Path.Combine(path, ".git")))
                return null;

            try
            {
                return new Repository(path);
            }
            catch
            {
                return null;
            }
        }
    }
}
