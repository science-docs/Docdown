using Docdown.Net;
using Docdown.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;

namespace Docdown.Model
{
    public class Workspace : IWorkspace
    {
        public string Name => Item.Name;
        public IFileSystem FileSystem { get; }
        public IWorkspaceItem Item { get; private set; }
        public IWorkspaceItem SelectedItem { get; set; }
        public IConverterService ConverterService { get; }
        public ConverterType FromType => FromSelectedItem();
        public ConverterType ToType { get; set; }
        public WorkspaceSettings Settings { get; }
        public List<IWorkspaceItemHandler> Handlers { get; } = new List<IWorkspaceItemHandler>();
        public Bibliography Bibliography { get; } = new Bibliography();
        public event WorkspaceChangeEventHandler WorkspaceChanged;

        private readonly Queue<FileSystemEventArgs> changeQueue = new Queue<FileSystemEventArgs>();
        private readonly Action debouncedChange;
        private IFileSystemWatcher watcher;
        private readonly List<string> excludedDirs = new List<string>();

        public Workspace(WorkspaceSettings settings, IFileSystem fileSystem, IConverterService converterService)
        {
            FileSystem = fileSystem;
            Settings = settings;
            ConverterService = converterService;

            debouncedChange = UIUtility.Debounce(ExecuteChanges, 1000);
        }

        public async Task Initialize()
        {
            var webHandler = Handlers.OfType<WebWorkspaceItemHandler>().FirstOrDefault();

            if (webHandler != null)
            {
                await DownloadWorkspace(webHandler);
            }

            var dir = FileSystem.DirectoryInfo.FromDirectoryName(Settings.Path);
            Item = new WorkspaceItem(dir, this, null);

            if (webHandler == null)
                Item.Type = WorkspaceItemType.Directory;
            else
                Item.Type = WorkspaceItemType.Web;


            foreach (var info in dir.EnumerateFileSystemInfos())
            {
                InitItem(info, Item, webHandler);
            }
            SetupFileSystemWatcher();
        }

        private void SetupFileSystemWatcher()
        {
            watcher = FileSystem.FileSystemWatcher.FromPath(Settings.Path);
            watcher.IncludeSubdirectories = true;
            watcher.EnableRaisingEvents = true;

            watcher.Changed += QueueChange;
            watcher.Created += QueueChange;
            watcher.Deleted += QueueChange;
            watcher.Renamed += QueueChange;
        }

        private void QueueChange(object obj, FileSystemEventArgs args)
        {
            if (excludedDirs.Any(e => args.FullPath.StartsWith(e)))
            {
                return;
            }

            changeQueue.Enqueue(args);
            debouncedChange();
        }

        private async void ExecuteChanges()
        {
            if (WorkspaceChanged == null)
            {
                changeQueue.Clear();
                return;
            }

            var eventDictionary = new Dictionary<string, WorkspaceChangeEventArgs>();

            while (changeQueue.Count > 0)
            {
                var change = changeQueue.Dequeue();
                var item = await IdentifyItem(change);
                if (item != null && change.ChangeType != WatcherChangeTypes.Changed)
                {
                    var changeValue = (WorkspaceChange)(int)change.ChangeType;
                    var args = new WorkspaceChangeEventArgs(item, changeValue);
                    if (!eventDictionary.TryGetValue(args.Item.FullName, out var oldArgs) || oldArgs.Change == WorkspaceChange.Deleted)
                    {
                        eventDictionary[args.Item.FullName] = args;
                    }
                }
            }

            WorkspaceChanged(this, eventDictionary.Values);
            OnWorkspaceChanged(eventDictionary.Values);
        }

        /// <summary>
        /// Remove leftover deleted items. The <see cref="IdentifyItem(FileSystemEventArgs)"/> method should create/rename all other items.
        /// </summary>
        /// <param name="args"></param>
        private void OnWorkspaceChanged(IEnumerable<WorkspaceChangeEventArgs> args) 
        {
            foreach (var arg in args.Where(e => e.Change == WorkspaceChange.Deleted))
            {
                var item = arg.Item;
                if (item.Parent != null)
                {
                    item.Parent.Children.Remove(item);
                    item.Parent = null;
                }
            }
        }

        /// <summary>
        /// Finds the item that belongs to the specified file system event.
        /// If it doesn't exist yet, it will be created.
        /// Does not do anything for delete events.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task<IWorkspaceItem> IdentifyItem(FileSystemEventArgs e)
        {
            var path = e.FullPath;
            if (e is RenamedEventArgs renamed)
            {
                path = renamed.OldFullPath;
                var item = FindItem(path, true);
                if (item != null && item.FullName != e.FullPath)
                {
                    if (item.IsDirectory)
                    {
                        item.FileInfo = FileSystem.DirectoryInfo.FromDirectoryName(e.FullPath);
                    }
                    else if (item.IsFile)
                    {
                        item.FileInfo = FileSystem.FileInfo.FromFileName(e.FullPath);
                    }
                    await item.Update();
                }
                return item;
            }
            else
            {
                return FindItem(path, true);
            }
        }

        private Task DownloadWorkspace(WebWorkspaceItemHandler handler)
        {
            return Task.CompletedTask;
            //var req = await WebUtility.GetRequest(WebUtility.BuildWorkspaceUrl(), MultipartFormParameter.FromWebWorkspace(this, handler.User));
            //var json = await req.Content.ReadAsStringAsync();

            //var obj = JObject.Parse(json);

            //var items = obj.SelectToken("items");

            //foreach (var item in items)
            //{
            //    var path = item.SelectToken("path").Value<string>();
            //    var dateString = item.SelectToken("date").Value<string>();
            //    var date = DateTime.Parse(dateString, null, DateTimeStyles.RoundtripKind);

            //    var filePath = Path.Combine(Settings.Path, path);

            //    var lastWrite = DateTime.MinValue;
            //    if (FileSystem.File.Exists(filePath))
            //    {
            //        lastWrite = FileSystem.File.GetLastWriteTimeUtc(filePath);
            //    }

            //    if (date > lastWrite)
            //    {
            //        var itemReq = await WebUtility.GetRequest(WebUtility.BuildWorkspaceItemUrl(), 
            //            MultipartFormParameter.FromWebWorkspace(this, handler.User).Concat(MultipartFormParameter.CreateField("name", path)));

            //        var parent = Path.GetDirectoryName(filePath);
            //        Directory.CreateDirectory(parent);
            //        using (var fs = FileSystem.File.Open(filePath, FileMode.Create))
            //        {
            //            await itemReq.Content.CopyToAsync(fs);
            //        }
            //    }
            //}
        }

        private void InitItem(IFileSystemInfo fileInfo, IWorkspaceItem parent, WebWorkspaceItemHandler handler)
        {
            if (fileInfo.Name.StartsWith("."))
            {
                if (fileInfo.Attributes.HasFlag(FileAttributes.Directory))
                    excludedDirs.Add(fileInfo.FullName + "\\");

                return;
            }

            var item = new WorkspaceItem(fileInfo, this, parent);

            if (fileInfo is IDirectoryInfo dirInfo)
            {
                item.Type = WorkspaceItemType.Directory;
                foreach (var info in dirInfo.EnumerateFileSystemInfos())
                {
                    InitItem(info, item, handler);
                }
            }
            else if (item.Type == WorkspaceItemType.Bibliography)
            {
                Bibliography.Parse(item);
            }

            parent.Children.Add(item);
        }

        private ConverterType FromSelectedItem()
        {
            if (SelectedItem is null)
                return ConverterType.Text;

            switch (SelectedItem.Type)
            {
                case WorkspaceItemType.Markdown:
                    return ConverterType.Markdown;
                case WorkspaceItemType.Latex:
                    return ConverterType.Latex;
                default:
                    return ConverterType.Text;
            }
        }

        private IWorkspaceItem FindItem(string fullPath, bool createItem)
        {
            var normalized = fullPath.Replace('/', '\\');
            return FindItem(Item, normalized, createItem);
        }

        private IWorkspaceItem FindItem(IWorkspaceItem parent, string fullPath, bool createItem)
        {
            var normalized = parent.FullName;
            if (normalized == fullPath)
            {
                return parent;
            }

            var parentPath = Path.GetDirectoryName(fullPath);
            bool isParent = parentPath == normalized;

            foreach (var child in parent.Children)
            {
                var item = FindItem(child, fullPath, createItem);
                if (item != null)
                {
                    return item;
                }
            }

            if (isParent && createItem)
            {
                // At that point we know, that although the item is logically
                // a child of the current item, it was not registered as a
                // part of the workspace, meaning it was created just now.
                IFileSystemInfo info;
                if (FileSystem.File.Exists(fullPath))
                {
                    info = FileSystem.FileInfo.FromFileName(fullPath);
                }
                else if (FileSystem.Directory.Exists(fullPath))
                {
                    info = FileSystem.DirectoryInfo.FromDirectoryName(fullPath);
                }
                else
                {
                    return null;
                }
                var item = new WorkspaceItem(info, this, parent);
                parent.Children.Add(item);
                return item;
            }

            return null;
        }

        public async Task<string> Convert(CancelToken cancelToken)
        {
            string path = null;
            var iterator = Handlers.GetEnumerator();
            while (path == null && iterator.MoveNext())
            {
                path = await iterator.Current.Convert(this, cancelToken);
            }
            return path;
        }

        public IWorkspaceItem FindRelativeItem(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                throw new ArgumentException("The relative path has to be an non empty string", nameof(relativePath));
            }

            var splits = relativePath.Split('\\', '/');

            var lastItem = Item;
            if (lastItem.Name != splits[0])
            {
                return null;
            }
            for (int i = 1; i < splits.Length; i++)
            {
                var name = splits[i];
                lastItem = FindNextRelativeItem(lastItem, name);

                if (lastItem == null)
                {
                    return null;
                }
            }
            return lastItem;
        }

        private IWorkspaceItem FindNextRelativeItem(IWorkspaceItem item, string name)
        {
            return item.Children.FirstOrDefault(e => e.Name == name);
        }

        public override string ToString()
        {
            return Item?.ToString() ?? base.ToString();
        }
    }
}
