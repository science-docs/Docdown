using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Docdown.Model
{
    public interface IWorkspace
    {
        IWorkspaceItem Item { get; }
        IWorkspaceItem SelectedItem { get; set; }
        ConverterType FromType { get; }
        ConverterType ToType { get; set; }
        event WorkspaceChangeEventHandler WorkspaceChanged;
    }
    public interface IWorkspace<T> : IWorkspace where T : class, IWorkspaceItem<T>
    {
        new T Item { get; }
        new T SelectedItem { get; set; }
    }
}
