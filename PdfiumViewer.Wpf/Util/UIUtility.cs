using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace PdfiumViewer.Wpf.Util
{
    internal static class UIUtility
    {
        public static Action Debounce(this Action func, int milliseconds = 300)
        {
            var action = Debounce<int>(_ => func(), milliseconds);
            return () => action(0);
        }

        public static Action<T> Debounce<T>(this Action<T> func, int milliseconds = 300)
        {
            var last = long.MinValue;
            return arg =>
            {
                try
                {
                    var current = System.Threading.Interlocked.Increment(ref last);
                    Task.Delay(milliseconds).ContinueWith(task =>
                    {
                        // ReSharper disable once AccessToModifiedClosure
                        if (current == last) func(arg);
                        task.Dispose();
                    });
                }
                catch (OverflowException)
                {
                    System.Threading.Interlocked.Exchange(ref last, long.MinValue);
                }
            };
        }

        public static T GetDescendantByType<T>(this Visual element) where T : Visual
        {
            return element.GetDescendantBy((T e) => true);
        }

        public static T GetDescendantByName<T>(this Visual element, string name) where T : Visual
        {
            return element.GetDescendantBy((T e) => e.GetValue(FrameworkElement.NameProperty) as string == name);
        }

        public static T GetDescendantBy<T>(this Visual element, Func<T, bool> filter) where T : Visual
        {
            if (element is null) return default;
            if (element is T item && filter(item))
            {
                return item;
            }
            T foundElement = null;
            (element as FrameworkElement)?.ApplyTemplate();
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                var visual = VisualTreeHelper.GetChild(element, i) as Visual;
                foundElement = visual.GetDescendantBy(filter);
                if (foundElement != null) break;
            }
            return foundElement;
        }

        public static T GetParentByType<T>(this Visual element) where T : Visual
        {
            return element.GetParentBy((T e) => true);
        }

        public static T GetParentBy<T>(this Visual element, Func<T, bool> filter) where T : Visual
        {
            if (element is null) return default;
            if (element is T item && filter(item))
            {
                return item;
            }
            (element as FrameworkElement)?.ApplyTemplate();
            return GetParentBy(VisualTreeHelper.GetParent(element) as Visual, filter);
        }
    }
}
