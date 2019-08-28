using Docdown.ViewModel;
using System;
using System.IO.Abstractions;
using System.Threading.Tasks;

namespace Docdown.Model
{
    public static class WorkspaceProvider
    {
        public static async Task<IWorkspace> Create(string path)
        {
            return await Create(path, new FileSystem());
        }

        public static async Task<IWorkspace> Create(string path, IFileSystem fileSystem)
        {
            if (fileSystem == null)
            {
                throw new ArgumentNullException(nameof(fileSystem));
            }
            if (!fileSystem.Directory.Exists(path))
            {
                throw new ArgumentException("Path does not exist in specified file system: " + (path ?? "<null>"));
            }

            var settings = WorkspaceSettings.Create(path);
            var workspace = new Workspace(settings, fileSystem);
            workspace.Handlers.Add(new FileWorkspaceItemHandler());
            if (!string.IsNullOrEmpty(settings.Sync))
            {
                var user = AppViewModel.Instance.User;
                if (user.EnsureLoggedIn())
                {
                    var webHandler = new WebWorkspaceItemHandler(user.Data);
                    workspace.Handlers.Add(webHandler);
                }
            }

            await workspace.Initialize();

            return workspace;
        }
    }
}