using Docdown.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

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

    public abstract class WorkspaceItem<T> : IWorkspaceItem<T>, IEquatable<WorkspaceItem<T>> where T : WorkspaceItem<T>
    {
        public abstract string Name { get; }
        public abstract string FullName { get; }
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
        public WorkspaceItemType Type { get; set; }
        public List<T> Children { get; } = new List<T>();
        public T Parent { get; set; }
        public T TopParent => Parent ?? this as T;
        public ConverterType FromType => FromFileType();
        public ConverterType ToType { get; set; } = ConverterType.Pdf;
        //public bool IsHidden => (FileSystemInfo.Attributes & FileAttributes.Hidden) > 0 || (Parent != null && Parent.IsHidden);
        
        public abstract Task<string> Convert(CancelToken cancelToken);

        public abstract Task Delete();

        public abstract Task<byte[]> Read();

        public abstract Task Save(string text);

        public abstract Task Rename(string newName);

        public abstract Task Update();

        public abstract Task<T> CreateNewFile(string name, string autoExtension = null, byte[] content = null);

        public abstract Task<T> CreateNewDirectory(string name);

        public abstract Task<T> CopyExistingItem(string path);

        public async virtual Task<T> CopyExistingFolder(string path)
        {
            var item = await CreateNewDirectory(Path.GetFileName(path));
            foreach (var file in Directory.GetFiles(path))
            {
                var child = await CopyExistingItem(file);
                item.Children.Add(child);
                child.Parent = item;
            }
            foreach (var directory in Directory.GetDirectories(path))
            {
                var child = await CopyExistingFolder(directory);
                item.Children.Add(child);
                child.Parent = item;
            }
            return item;
        }

        public bool IsDirectory => Type == WorkspaceItemType.Directory || Type == WorkspaceItemType.Web;

        public bool IsFile => !IsDirectory;

        IReadOnlyCollection<IWorkspaceItem> IWorkspaceItem.Children => Children.AsReadOnly();

        IWorkspaceItem IWorkspaceItem.Parent { get => Parent; set => Parent = (T)value; }

        IWorkspaceItem IWorkspaceItem.TopParent => TopParent;

        async Task<IWorkspaceItem> IWorkspaceItem.CreateNewFile(string name, string autoExtension, byte[] content)
        {
            return await CreateNewFile(name, autoExtension, content);
        }

        async Task<IWorkspaceItem> IWorkspaceItem.CreateNewDirectory(string name)
        {
            return await CreateNewDirectory(name);
        }

        async Task<IWorkspaceItem> IWorkspaceItem.CopyExistingItem(string path)
        {
            return await CopyExistingItem(path);
        }

        async Task<IWorkspaceItem> IWorkspaceItem.CopyExistingFolder(string path)
        {
            return await CopyExistingFolder(path);
        }

        public override string ToString()
        {
            return RelativeName ?? base.ToString();
        }

        public override bool Equals(object obj)
        {
            if (obj is WorkspaceItem<T> item)
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

        public bool Equals(WorkspaceItem<T> other)
        {
            return ToString() == other?.ToString();
        }

        protected void SetType(string fileExt)
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