using System;
using System.Windows;
using System.Windows.Media;

namespace PdfiumViewer.Wpf.Util
{
    internal static class UIUtility
    {
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
            if (element == null) return default;
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
            if (element == null) return default;
            if (element is T item && filter(item))
            {
                return item;
            }
            (element as FrameworkElement)?.ApplyTemplate();
            return GetParentBy(VisualTreeHelper.GetParent(element) as Visual, filter);
        }
    }
}
