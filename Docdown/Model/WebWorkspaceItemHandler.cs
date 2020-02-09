using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Docdown.Util;

namespace Docdown.Model
{
    public class WebWorkspaceItemHandler : IWorkspaceItemHandler
    {
        public int Order => 1;
        public User User { get; }

        public WebWorkspaceItemHandler(User user)
        {
            User = user;
        }

        public Task<string> Convert(IWorkspace workspace, CancelToken cancelToken)
        {
            throw new NotImplementedException();
        }

        public Task Delete(IWorkspaceItem item)
        {
            throw new NotImplementedException();
        }

        public byte[] Read(IWorkspaceItem item)
        {
            throw new NotImplementedException();
        }

        public Task Rename(IWorkspaceItem item, string newName)
        {
            throw new NotImplementedException();
        }

        public Task Save(IWorkspaceItem item, byte[] bytes)
        {
            throw new NotImplementedException();
        }
    }
}
