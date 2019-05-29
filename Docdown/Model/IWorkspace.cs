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
