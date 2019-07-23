using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Docdown.ViewModel
{
    public class ObservableList<T> : ObservableObject, ICollection<T>, INotifyCollectionChanged
    {
        private readonly ObservableCollection<T> collection = new ObservableCollection<T>();

        public int Count => collection.Count;

        public bool IsReadOnly => false;

        public T this[int index]
        {
            get => collection[index];
            set => collection[index] = value;
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add
            {
                collection.CollectionChanged += value;
            }
            remove
            {
                collection.CollectionChanged -= value;
            }
        }

        public void Add(T item)
        {
            collection.Add(item);
        }

        public void Clear()
        {
            collection.Clear();
        }

        public bool Contains(T item)
        {
            return collection.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            collection.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return collection.GetEnumerator();
        }

        public bool Remove(T item)
        {
            return collection.Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return collection.GetEnumerator();
        }
    }
}
