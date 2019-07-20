using System;
using System.Linq;

namespace Docdown.Model
{
    public abstract class Workspace<T> : IWorkspace<T> where T: class, IWorkspaceItem<T>
    {
        public abstract T Item { get; }
        public abstract T SelectedItem { get; set; }
        public ConverterType FromType => FromSelectedItem();
        public ConverterType ToType { get; set; } = ConverterType.Pdf;
        public abstract event WorkspaceChangeEventHandler WorkspaceChanged;

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

        public T FindRelativeItem(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                throw new ArgumentException("The relative path has to be an non empty string", nameof(relativePath));
            }

            var splits = relativePath.Split('\\', '/');

            var lastItem = Item;
            if (lastItem.Name != splits[0])
            {
                return null;
            }
            for (int i = 1; i < splits.Length; i++)
            {
                var name = splits[i];
                lastItem = FindNextRelativeItem(lastItem, name);

                if (lastItem == null)
                {
                    return null;
                }
            }
            return lastItem;
        }

        private T FindNextRelativeItem(T item, string name)
        {
            return item.Children.FirstOrDefault(e => e.Name == name);
        }

        public override string ToString()
        {
            return Item?.ToString() ?? base.ToString();
        }
    }
}
