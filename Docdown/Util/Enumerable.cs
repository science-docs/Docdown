using System;
using System.Collections.Generic;
using System.Linq;

namespace Docdown.Util
{
    public interface IExpandable<T> where T : IExpandable<T>
    {
        bool IsExpanded { get; set; }
        IEnumerable<T> Children { get; }
    }

    public static class Enumerable
    {
        public static T FirstOrDefault<T>(this T source, Func<T, IEnumerable<T>> recursive, Func<T, bool> selector)
        {
            if (selector(source))
            {
                return source;
            }
            foreach (var child in recursive(source))
            {
                var found = FirstOrDefault(child, recursive, selector);
                if (found != null && !found.Equals(default))
                {
                    return found;
                }
            }
            return default;
        }

        public static IEnumerable<T> Concat<T>(this IEnumerable<T> source, T item)
        {
            foreach (var value in source)
            {
                yield return value;
            }
            yield return item;
        }

        public static IEnumerable<T> Except<T>(this IEnumerable<T> source, T item)
        {
            return source.Where(e => !e.Equals(item));
        }

        public static void Restore<T>(this IEnumerable<T> own, IEnumerable<T> other) where T : class, IExpandable<T>, IComparable<T>
        {
            Restore(own, other, (a, b) => a.CompareTo(b));
        }

        public static void Restore<T>(this IEnumerable<T> own,
            IEnumerable<T> other, Comparison<T> comparer) where T : class, IExpandable<T>
        {
            Restore(own, other, e => e.IsExpanded, e => e.Children, comparer, e => e.IsExpanded = true);
        }

        public static void Restore<T>(this IEnumerable<T> own,
            IEnumerable<T> other,
            Func<T, bool> filter,
            Func<T, IEnumerable<T>> children,
            Comparison<T> comparer,
            Action<T> foundAction) where T : class
        {
            if (own is null)
                throw new ArgumentNullException(nameof(own));
            if (other is null)
                throw new ArgumentNullException(nameof(other));
            if (filter is null)
                throw new ArgumentNullException(nameof(filter));
            if (children is null)
                throw new ArgumentNullException(nameof(children));
            if (comparer is null)
                throw new ArgumentNullException(nameof(comparer));
            if (foundAction is null)
                throw new ArgumentNullException(nameof(foundAction));

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

        private static T SearchTreeByName<T>(IEnumerable<T> items, T item, Func<T, IEnumerable<T>> children, Comparison<T> comparer) where T : class
        {
            T found = null;
            foreach (var val in items)
            {
                if (comparer(val, item) == 0)
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
