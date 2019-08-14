using Docdown.ViewModel;
using System.Threading.Tasks;

namespace Docdown.Model
{
    public static class WorkspaceProvider
    {
        public static async Task<IWorkspace> Create(string path)
        {
            var settings = WorkspaceSettings.Create(path);
            var workspace = new Workspace(settings);
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