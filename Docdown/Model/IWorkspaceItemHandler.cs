﻿using Docdown.Util;
using System.Threading.Tasks;

namespace Docdown.Model
{
    public interface IWorkspaceItemHandler
    {
        int Order { get; }

        Task<byte[]> Read(IWorkspaceItem item);
        Task Save(IWorkspaceItem item, byte[] bytes);
        Task<string> Convert(IWorkspaceItem item, CancelToken cancelToken);
        Task Delete(IWorkspaceItem item);
        Task Rename(IWorkspaceItem item, string newName);
    }
}