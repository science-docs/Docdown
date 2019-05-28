using Docdown.Util;
using LibGit2Sharp;
using System;
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
        public event EventHandler WorkspaceChanged;

        private readonly FileSystemWatcher watcher;

        private readonly Action debounceUpdate;

        public FileWorkspace(string path)
        {
            Repository = SetupGit(path);
            Item = new FileWorkspaceItem(path);

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
                WorkspaceChanged?.Invoke(this, EventArgs.Empty);
            }, 1000);
        }

        private void OnWorkspaceChanged(object sender, FileSystemEventArgs e)
        {
            debounceUpdate();
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
