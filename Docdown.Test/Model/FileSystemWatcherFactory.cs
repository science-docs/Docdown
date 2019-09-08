using System.ComponentModel;
using System.IO;
using System.IO.Abstractions;

namespace Docdown.Model.Test
{
    public class FileSystemWatcherFactory : IFileSystemWatcherFactory
    {
        private static readonly MockFileSystemWatcher Instance = new MockFileSystemWatcher();

        public void Trigger(FileSystemEventArgs e)
        {
            switch (e.ChangeType)
            {
                case WatcherChangeTypes.Created:
                    Instance.Create(e);
                    break;
                case WatcherChangeTypes.Deleted:
                    Instance.Delete(e);
                    break;
                case WatcherChangeTypes.Changed:
                    Instance.Change(e);
                    break;
                case WatcherChangeTypes.Renamed:
                    Instance.Rename(e);
                    break;
            }
        }

        public IFileSystemWatcher CreateNew()
        {
            return Instance;
        }

        public IFileSystemWatcher FromPath(string path)
        {
            return Instance;
        }

        class MockFileSystemWatcher : IFileSystemWatcher
        {
            public bool IncludeSubdirectories { get; set; }
            public bool EnableRaisingEvents { get; set; }
            public string Filter { get; set; }
            public int InternalBufferSize { get; set; }
            public NotifyFilters NotifyFilter { get; set; }
            public string Path { get; set; }
            public ISite Site { get; set; }
            public ISynchronizeInvoke SynchronizingObject { get; set; }

            public event FileSystemEventHandler Changed;
            public event FileSystemEventHandler Created;
            public event FileSystemEventHandler Deleted;
            public event ErrorEventHandler Error;
            public event RenamedEventHandler Renamed;

            public void Create(FileSystemEventArgs e)
            {
                Created?.Invoke(this, e);
            }

            public void Change(FileSystemEventArgs e)
            {
                Changed?.Invoke(this, e);
            }

            public void Delete(FileSystemEventArgs e)
            {
                Deleted?.Invoke(this, e);
            }
            
            public void Rename(FileSystemEventArgs e)
            {
                Renamed?.Invoke(this, e as RenamedEventArgs);
            }

            public void Err()
            {
                Error?.Invoke(null, null);
            }

            public void BeginInit()
            {

            }

            public void Dispose()
            {

            }

            public void EndInit()
            {

            }

            public WaitForChangedResult WaitForChanged(WatcherChangeTypes changeType)
            {
                return new WaitForChangedResult();
            }

            public WaitForChangedResult WaitForChanged(WatcherChangeTypes changeType, int timeout)
            {
                return new WaitForChangedResult();
            }
        }
    }
}
