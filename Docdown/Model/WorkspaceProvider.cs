using Docdown.ViewModel;
using System;
using System.Linq;
using System.Net;

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
                    return LoadWebWorkspace(uri);
                case "file":
                    return new FileWorkspace(WebUtility.UrlDecode(uri.AbsolutePath));
            }
            throw new ArgumentException("Could not parse URI scheme: " + uri.Scheme);
        }

        private static WebWorkspace LoadWebWorkspace(Uri uri)
        {
            var user = AppViewModel.Instance.User.Data;
            var lastSegment = uri.Segments.Last();
            var decodedName = WebUtility.UrlDecode(lastSegment);
            return new WebWorkspace(user, decodedName);
        }
    }
}