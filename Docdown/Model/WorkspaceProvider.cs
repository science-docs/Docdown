namespace Docdown.Model
{
    public static class WorkspaceProvider
    {
        public static IWorkspace Create(string path)
        {
            return new FileWorkspace(path);
        }
    }
}
