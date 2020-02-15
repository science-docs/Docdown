using Docdown.Net;
using System.Threading.Tasks;

namespace Docdown.Model
{
    public interface IWorkspaceItemHandler
    {
        int Order { get; }

        byte[] Read(IWorkspaceItem item);
        Task Save(IWorkspaceItem item, byte[] bytes);
        Task<string> Convert(IWorkspace workspace, CancelToken cancelToken);
        Task Delete(IWorkspaceItem item);
        Task Rename(IWorkspaceItem item, string newName);
    }
}
