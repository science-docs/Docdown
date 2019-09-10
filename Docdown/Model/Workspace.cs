using Docdown.Util;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
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

        private void QueueChange(object obj, FileSystemEventArgs e)
        {
            changeQueue.Enqueue(e);
            debouncedChange();
        }

        private async void ExecuteChanges()
        {
            if (WorkspaceChanged == null)
            {
                changeQueue.Clear();
                return;
            }

            var eventArgs = new List<WorkspaceChangeEventArgs>();

            while (changeQueue.Count > 0)
            {
                var change = changeQueue.Dequeue();
                var item = await IdentifyItem(change);
                var changeValue = (WorkspaceChange)(int)change.ChangeType;
                var args = new WorkspaceChangeEventArgs(item, changeValue);
                eventArgs.Add(args);
            }

            WorkspaceChanged(this, eventArgs);
        }

        private async Task<IWorkspaceItem> IdentifyItem(FileSystemEventArgs e)
        {
            var path = e.FullPath;
            if (e is RenamedEventArgs renamed)
            {
                path = renamed.OldFullPath;
                var item = FindItem(path);
                if (item.FullName != e.FullPath)
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
                return FindItem(path);
            }
        }

        private async Task DownloadWorkspace(WebWorkspaceItemHandler handler)
        {
            var req = await WebUtility.GetRequest(WebUtility.BuildWorkspaceUrl(), MultipartFormParameter.FromWebWorkspace(this, handler.User));
            var json = await req.Content.ReadAsStringAsync();

            var obj = JObject.Parse(json);

            var items = obj.SelectToken("items");

            foreach (var item in items)
            {
                var path = item.SelectToken("path").Value<string>();
                var dateString = item.SelectToken("date").Value<string>();
                var date = DateTime.Parse(dateString, null, DateTimeStyles.RoundtripKind);

                var filePath = Path.Combine(Settings.Path, path);

                var lastWrite = DateTime.MinValue;
                if (File.Exists(filePath))
                {
                    lastWrite = File.GetLastWriteTimeUtc(filePath);
                }

                if (date > lastWrite)
                {
                    var itemReq = await WebUtility.GetRequest(WebUtility.BuildWorkspaceItemUrl(), 
                        MultipartFormParameter.FromWebWorkspace(this, handler.User).Concat(MultipartFormParameter.CreateField("name", path)));

                    var parent = Path.GetDirectoryName(filePath);
                    Directory.CreateDirectory(parent);
                    using (var fs = File.Open(filePath, FileMode.Create))
                    {
                        await itemReq.Content.CopyToAsync(fs);
                    }
                }
            }
        }

        private void InitItem(IFileSystemInfo fileInfo, IWorkspaceItem parent, WebWorkspaceItemHandler handler)
        {
            var item = new WorkspaceItem(fileInfo, this, parent);

            if (fileInfo is IDirectoryInfo dirInfo)
            {
                item.Type = WorkspaceItemType.Directory;
                foreach (var info in dirInfo.EnumerateFileSystemInfos().Where(e => !e.Name.StartsWith(".")))
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

        private IWorkspaceItem FindItem(string fullPath)
        {
            var normalized = fullPath.Replace('/', '\\');
            return FindItem(Item, normalized);
        }

        private IWorkspaceItem FindItem(IWorkspaceItem parent, string fullPath)
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
                var item = FindItem(child, fullPath);
                if (item != null)
                {
                    return item;
                }
            }

            if (isParent)
            {
                // At that point we know, that although the item is logically
                // a child of the current item, it was not registered as a
                // part of the workspace, meaning it was created just now.
                var info = FileSystem.FileInfo.FromFileName(fullPath);
                var item = new WorkspaceItem(info, this, parent);
                parent.Children.Add(item);
                return item;
            }

            return null;
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
