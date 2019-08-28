using Docdown.Util;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Path = System.IO.Path;

namespace Docdown.Model
{
    public enum WorkspaceItemType
    {
        Other,
        Bibliography,
        Markdown,
        Latex,
        Pdf,
        Directory,
        Image,
        Video,
        Audio,
        Text,
        Docx,
        Web
    }

    public class WorkspaceItem : IWorkspaceItem
    {
        public string Name => FileInfo.Name;
        public string FullName => FileInfo.FullName;
        public string RelativeName
        {
            get
            {
                string name = Name;
                if (Parent != null)
                {
                    name = Parent.RelativeName + "/" + name;
                }
                return name;
            }
        }

        public IFileSystemInfo FileInfo
        {
            get => fileInfo;
            set
            {
                fileInfo = value;
                SetType(fileInfo.Extension);
            }
        }

        public WorkspaceItemType Type { get; set; }
        public List<IWorkspaceItem> Children { get; } = new List<IWorkspaceItem>();
        public IWorkspaceItem Parent { get; set; }
        public IWorkspaceItem TopParent => Parent ?? this;
        public ConverterType FromType => FromFileType();
        public ConverterType ToType { get; set; } = ConverterType.Pdf;

        public IWorkspace Workspace { get; set; }

        public bool IsDirectory => Type == WorkspaceItemType.Directory || Type == WorkspaceItemType.Web;

        public bool IsFile => !IsDirectory;

        private IFileSystemInfo fileInfo;
        private IFileSystem FS => fileInfo.FileSystem;

        public WorkspaceItem(IFileSystemInfo info, IWorkspace workspace, IWorkspaceItem parent)
        {
            FileInfo = info;
            Parent = parent;
            Workspace = workspace;
        }

        public async Task<string> Convert(CancelToken cancelToken)
        {
            string path = null;
            var iterator = Workspace.Handlers.GetEnumerator();
            while (path == null && iterator.MoveNext())
            {
                path = await iterator.Current.Convert(this, cancelToken);
            }
            return path;
        }

        public async Task<IWorkspaceItem> CopyExistingFolder(string path)
        {
            var item = await CreateNewDirectory(Path.GetFileName(path));
            foreach (var file in FS.Directory.GetFiles(path))
            {
                var child = await CopyExistingItem(file);
                item.Children.Add(child);
                child.Parent = item;
            }
            foreach (var directory in FS.Directory.GetDirectories(path))
            {
                var child = await CopyExistingFolder(directory);
                item.Children.Add(child);
                child.Parent = item;
            }
            return item;
        }

        public async Task<IWorkspaceItem> CreateNewFile(string name, string autoExtension = null, byte[] content = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("message", nameof(name));
            if (Type != WorkspaceItemType.Directory)
                throw new InvalidOperationException("Only Directories can contain files");

            if (autoExtension != null && !name.EndsWith(autoExtension))
            {
                name += autoExtension;
            }

            string fullName = Path.Combine(FullName, name);

            if (content is null)
            {
                content = new byte[0];
            }

            if (FindChild(fullName, out var existing))
            {
                return existing;
            }

            var fileInfo = FS.FileInfo.FromFileName(fullName);
            var item = new WorkspaceItem(fileInfo, Workspace, this);
            await Task.WhenAll(Workspace.Handlers.Select(e => e.Save(item, content)));
            Children.Add(item);
            return item;
        }

        public Task<IWorkspaceItem> CreateNewDirectory(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Empty directory name invalid", nameof(name));
            if (Type != WorkspaceItemType.Directory)
                throw new InvalidOperationException("Only Directories can contain other directories");

            string fullName = Path.Combine(FullName, name);

            if (FindChild(fullName, out var existing))
            {
                return Task.FromResult(existing);
            }

            var fileInfo = FS.Directory.CreateDirectory(fullName);
            IWorkspaceItem item = new WorkspaceItem(fileInfo, Workspace, this);
            Children.Add(item);
            return Task.FromResult(item);
        }

        public Task<IWorkspaceItem> CopyExistingItem(string path)
        {
            var fileName = Path.GetFileName(path);
            var fullNewName = Path.Combine(FullName, fileName);

            if (Children.Any(e => e.Name == fileName))
            {
                FS.File.Replace(path, fullNewName, null);
            }
            else
            {
                FS.File.Copy(path, fullNewName);
            }

            var fileInfo = FS.FileInfo.FromFileName(fullNewName);
            IWorkspaceItem item = new WorkspaceItem(fileInfo, Workspace, this);
            Children.Add(item);
            return Task.FromResult(item);
        }

        public async Task Delete()
        {
            foreach (var handler in Workspace.Handlers)
            {
                await handler.Delete(this);
            }
        }

        public async Task<byte[]> Read()
        {
            byte[] bytes = null;
            var iterator = Workspace.Handlers.GetEnumerator();
            while (bytes == null && iterator.MoveNext())
            {
                bytes = await iterator.Current.Read(this);
            }
            return bytes;
        }

        public async Task Rename(string newName)
        {
            if (Parent == null)
            {
                var parent = Path.GetDirectoryName(Workspace.Settings.Path);
                var name = Path.Combine(parent, newName);
                Workspace.Settings.Path = name;
            }
            foreach (var handler in Workspace.Handlers)
            {
                await handler.Rename(this, newName);
            }
            await Update();
        }

        public async Task Save(string text)
        {
            var bytes = Encoding.UTF8.GetBytes(text);
            foreach (var handler in Workspace.Handlers)
            {
                await handler.Save(this, bytes);
            }
            if (Type == WorkspaceItemType.Bibliography)
            {
                Workspace.Bibliography.Parse(this, text);
            }
        }

        public async Task Update()
        {
            if (Parent != null)
            {
                var topName = Path.GetDirectoryName(TopParent.FullName);
                var fullPath = Path.Combine(topName, RelativeName);
                if (IsDirectory)
                {
                    FileInfo = FS.DirectoryInfo.FromDirectoryName(fullPath);
                }
                else
                {
                    FileInfo = FS.FileInfo.FromFileName(fullPath);
                }
            }
            foreach (var child in Children)
            {
                await child.Update();
            }
        }

        public override string ToString()
        {
            return RelativeName ?? base.ToString();
        }

        private bool FindChild(string fullName, out IWorkspaceItem item)
        {
            item = Children.FirstOrDefault(e => e.FullName == fullName);
            return item != null;
        }

        public override bool Equals(object obj)
        {
            if (obj is WorkspaceItem item)
            {
                return Equals(item);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return RelativeName?.GetHashCode() ?? base.GetHashCode();
        }

        private ConverterType FromFileType()
        {
            switch (Type)
            {
                case WorkspaceItemType.Markdown:
                    return ConverterType.Markdown;
                case WorkspaceItemType.Latex:
                    return ConverterType.Latex;
                default:
                    return ConverterType.Text;
            }
        }

        public bool Equals(WorkspaceItem other)
        {
            return ToString() == other?.ToString();
        }

        private void SetType(string fileExt)
        {
            switch (fileExt)
            {
                case ".md":
                case ".markdown":
                    Type = WorkspaceItemType.Markdown;
                    break;
                case ".tex":
                case ".latex":
                    Type = WorkspaceItemType.Latex;
                    break;
                case ".pdf":
                    Type = WorkspaceItemType.Pdf;
                    break;
                case ".docx":
                    Type = WorkspaceItemType.Docx;
                    break;
                case ".txt":
                case ".ini":
                case ".bat":
                case ".sh":
                case ".json":
                case ".xml":
                case ".xaml":
                    Type = WorkspaceItemType.Text;
                    break;
                case ".png":
                case ".jpg":
                case ".jpeg":
                case ".gif":
                case ".webp":
                    Type = WorkspaceItemType.Image;
                    break;
                case ".mp4":
                case ".webm":
                case ".mkv":
                    Type = WorkspaceItemType.Video;
                    break;
                case ".mp3":
                case ".flac":
                case ".ogg":
                    Type = WorkspaceItemType.Audio;
                    break;
                case ".bib":
                case ".bibtex":
                    Type = WorkspaceItemType.Bibliography;
                    break;
                default:
                    Type = WorkspaceItemType.Other;
                    break;
            }
        }
    }
}