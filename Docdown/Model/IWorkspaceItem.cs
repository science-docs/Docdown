using Docdown.Util;
using System.Collections.Generic;

namespace Docdown.Model
{
    public interface IWorkspaceItem
    {
        string RelativeName { get; }
        string Name { get; }
        WorkspaceItemType Type { get; set; }
        List<IWorkspaceItem> Children { get; }
        IWorkspaceItem Parent { get; set; }
        IWorkspaceItem TopParent { get; }
        ConverterType FromType { get; }
        ConverterType ToType { get; set; }
        //bool IsHidden { get; set; }
        bool IsDirectory { get; }
        bool IsFile { get; }

        byte[] Read();
        void Save(string text);
        string Convert(CancelToken cancelToken);
        void Delete();
        void Rename(string newName);
        void Update();
        IWorkspaceItem CreateNewFile(string name, string autoExtension = null, byte[] content = null);
        IWorkspaceItem CreateNewDirectory(string name);
        IWorkspaceItem CopyExistingItem(string path);
        IWorkspaceItem CopyExistingFolder(string path);
    }

    public interface IWorkspaceItem<T> : IWorkspaceItem where T : class, IWorkspaceItem<T>
    {
        new WorkspaceItemCollection<T> Children { get; }
        new T Parent { get; set; }
        new T TopParent { get; }
        new T CreateNewFile(string name, string autoExtension = null, byte[] content = null);
        new T CreateNewDirectory(string name);
        new T CopyExistingItem(string path);
        new T CopyExistingFolder(string path);
    }
}