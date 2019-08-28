using System.Collections.Generic;
using System.IO.Abstractions;

namespace Docdown.Model
{
    public interface IWorkspace
    {
        string Name { get; }
        IFileSystem FileSystem { get; }
        IWorkspaceItem Item { get; }
        IWorkspaceItem SelectedItem { get; set; }
        ConverterType FromType { get; }
        ConverterType ToType { get; set; }
        List<IWorkspaceItemHandler> Handlers { get; }
        Bibliography Bibliography { get; }
        WorkspaceSettings Settings { get; }
        event WorkspaceChangeEventHandler WorkspaceChanged;
    }
}
