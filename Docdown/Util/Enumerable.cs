using System;
using System.Collections.Generic;

namespace Docdown.Util
{
    public static class Enumerable
    {
        public static void Restore<T>(this IEnumerable<T> own, 
            IEnumerable<T> other, 
            Func<T, bool> filter, 
            Func<T, IEnumerable<T>> children,
            Func<T, T, bool> comparer, 
            Action<T> foundAction) where T : class
        {
            foreach (var child in own)
            {
                if (filter(child))
                {
                    var searchResult = SearchTreeByName(other, child, children, comparer);
                    if (searchResult != null)
                    {
                        foundAction(searchResult);
                    }
                    Restore(children(child), other, filter, children, comparer, foundAction);
                }
            }
        }

        private static T SearchTreeByName<T>(IEnumerable<T> items, T item, Func<T, IEnumerable<T>> children, Func<T, T, bool> comparer) where T : class
        {
            T found = null;
            foreach (var val in items)
            {
                if (comparer(val, item))
                {
                    return val;
                }
                else
                {
                    found = SearchTreeByName(children(val), item, children, comparer);
                }
            }
            return found;
        }
    }
}
