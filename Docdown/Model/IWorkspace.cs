using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Docdown.Util;

namespace Docdown.Model
{
    public interface IWorkspace
    {
        string Name { get; }
        IFileSystem FileSystem { get; }
        IWorkspaceItem Item { get; }
        IWorkspaceItem SelectedItem { get; set; }
        IConverterService ConverterService { get; }
        ConverterType FromType { get; }
        ConverterType ToType { get; set; }
        Task<string> Convert(CancelToken cancelToken);
        List<IWorkspaceItemHandler> Handlers { get; }
        Bibliography Bibliography { get; }
        WorkspaceSettings Settings { get; }
        event WorkspaceChangeEventHandler WorkspaceChanged;
    }

    public static class WorkspaceExtension
    {
        public static IWorkspaceItem FindBy(this IWorkspace workspace, Func<IWorkspaceItem, bool> predicate)
        {
            return Flatten(workspace).FirstOrDefault(predicate);
        }

        public static IWorkspaceItem FindByRelativeName(this IWorkspace workspace, string relativeName)
        {
            return workspace.FindBy(e => e.RelativeName == relativeName);
        }

        public static IWorkspaceItem FindByName(this IWorkspace workspace, string fullName)
        {
            return workspace.FindBy(e => e.FullName == fullName);
        }

        public static IEnumerable<IWorkspaceItem> Flatten(this IWorkspace workspace)
        {
            return Flatten(workspace.Item);
        }

        public static IEnumerable<IWorkspaceItem> Flatten(this IWorkspaceItem workspaceItem)
        {
            yield return workspaceItem;
            foreach (var children in workspaceItem.Children.SelectMany(e => Flatten(e)))
            {
                yield return children;
            }
        }
    }
}
