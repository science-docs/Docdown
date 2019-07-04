using Docdown.Util;
using System;
using System.Collections.Generic;

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
        Docx
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
        
        public abstract string Convert(CancelToken cancelToken);

        public abstract void Delete();

        public abstract byte[] Read();

        public abstract void Save(string text);

        public abstract void Rename(string newName);

        public abstract void Update();

        public abstract T CreateNewFile(string name, string autoExtension = null, byte[] content = null);

        public abstract T CreateNewDirectory(string name);

        public abstract T CopyExistingItem(string path);

        public abstract T CopyExistingFolder(string path);
        
        public bool IsDirectory => Type == WorkspaceItemType.Directory;

        public bool IsFile => !IsDirectory;

        IReadOnlyCollection<IWorkspaceItem> IWorkspaceItem.Children => Children.AsReadOnly();

        IWorkspaceItem IWorkspaceItem.Parent { get => Parent; set => Parent = (T)value; }

        IWorkspaceItem IWorkspaceItem.TopParent => TopParent;

        IWorkspaceItem IWorkspaceItem.CreateNewFile(string name, string autoExtension, byte[] content)
        {
            return CreateNewFile(name, autoExtension, content);
        }

        IWorkspaceItem IWorkspaceItem.CreateNewDirectory(string name)
        {
            return CreateNewDirectory(name);
        }

        IWorkspaceItem IWorkspaceItem.CopyExistingItem(string path)
        {
            return CopyExistingItem(path);
        }

        IWorkspaceItem IWorkspaceItem.CopyExistingFolder(string path)
        {
            return CopyExistingFolder(path);
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
    }
}