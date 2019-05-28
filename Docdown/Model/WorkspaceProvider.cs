using System;

namespace Docdown.Model
{
    public static class WorkspaceProvider
    {
        public static IWorkspace Create(Uri uri)
        {
            switch (uri.Scheme)
            {
                case "http":
                case "https":
                    throw new NotImplementedException("Online workspaces are WIP");
                case "file":
                    return new FileWorkspace(uri.AbsolutePath);
            }
            throw new ArgumentException("Could not parse URI scheme: " + uri.Scheme);
        }
    }
}