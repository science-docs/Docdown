namespace Docdown.Model
{
    public abstract class Workspace<T> : IWorkspace<T> where T: class, IWorkspaceItem<T>
    {
        public abstract T Item { get; }
        public abstract T SelectedItem { get; set; }
        public ConverterType FromType => FromSelectedItem();
        public ConverterType ToType { get; set; }

        IWorkspaceItem IWorkspace.Item => Item;

        IWorkspaceItem IWorkspace.SelectedItem { get => SelectedItem; set => SelectedItem = (T)value; }

        private ConverterType FromSelectedItem()
        {
            if (SelectedItem is null)
                return ConverterType.Text;

            switch (SelectedItem.Type)
            {
                case WorkspaceItemType.Markdown:
                    return ConverterType.Markdown;
                case WorkspaceItemType.Latex:
                    return ConverterType.Latex;
                default:
                    return ConverterType.Text;
            }
        }

        public override string ToString()
        {
            return Item?.ToString() ?? base.ToString();
        }
    }
}
