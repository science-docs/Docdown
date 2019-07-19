using Docdown.Util;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Docdown.Model
{
    public interface IWorkspaceItem
    {
        string RelativeName { get; }
        string Name { get; }
        string FullName { get; }
        WorkspaceItemType Type { get; set; }
        IReadOnlyCollection<IWorkspaceItem> Children { get; }
        IWorkspaceItem Parent { get; set; }
        IWorkspaceItem TopParent { get; }
        ConverterType FromType { get; }
        ConverterType ToType { get; set; }
        //bool IsHidden { get; set; }
        bool IsDirectory { get; }
        bool IsFile { get; }

        Task<byte[]> Read();
        Task Save(string text);
        Task<string> Convert(CancelToken cancelToken);
        Task Delete();
        Task Rename(string newName);
        Task Update();
        Task<IWorkspaceItem> CreateNewFile(string name, string autoExtension = null, byte[] content = null);
        Task<IWorkspaceItem> CreateNewDirectory(string name);
        Task<IWorkspaceItem> CopyExistingItem(string path);
        Task<IWorkspaceItem> CopyExistingFolder(string path);
    }

    public interface IWorkspaceItem<T> : IWorkspaceItem where T : class, IWorkspaceItem<T>
    {
        new List<T> Children { get; }
        new T Parent { get; set; }
        new T TopParent { get; }
        new Task<T> CreateNewFile(string name, string autoExtension = null, byte[] content = null);
        new Task<T> CreateNewDirectory(string name);
        new Task<T> CopyExistingItem(string path);
        new Task<T> CopyExistingFolder(string path);
    }
}