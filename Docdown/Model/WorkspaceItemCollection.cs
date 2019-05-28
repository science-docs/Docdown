using System;
using System.Collections;
using System.Collections.Generic;

namespace Docdown.Model
{
    public class WorkspaceItemCollection<T> : ICollection<T> where T : class, IWorkspaceItem<T>
    {
        public int Count => items.Count;

        public bool IsReadOnly => false;

        public T Parent { get; set; }

        private readonly List<T> items;

        public WorkspaceItemCollection(T parentItem)
        {
            Parent = parentItem;
            items = new List<T>();
        }

        public void Add(T item)
        {
            items.Add(item);
            GetBaseChildren().Add(item);
        }

        public void AddRange(IEnumerable<T> items)
        {
            this.items.AddRange(items);
            GetBaseChildren().AddRange(items);
        }

        public void Clear()
        {
            items.Clear();
            GetBaseChildren().Clear();
        }

        public bool Contains(T item)
        {
            return items.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return items.GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return items.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            items.Insert(index, item);
            GetBaseChildren().Insert(index, item);
        }

        public bool Remove(T item)
        {
            bool result = items.Remove(item);
            if (result)
            {
                GetBaseChildren().Remove(item);
            }
            return result;
        }

        public void RemoveAt(int index)
        {
            items.RemoveAt(index);
            GetBaseChildren().RemoveAt(index);
        }

        

        private List<IWorkspaceItem> GetBaseChildren()
        {
            var item = (IWorkspaceItem)Parent;
            return item.Children;
        }

        public void ForEach(Action<T> action)
        {
            items.ForEach(action);
        }
    }
}
