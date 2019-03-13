using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace Docdown.Util
{
    public static class UIUtility
    {
        public static void Delay(this DispatcherObject dispatcherObject, int milliseconds, Action syncAction)
        {
            dispatcherObject.Delay(MillisecondDelay, syncAction);

            void MillisecondDelay()
            {
                var delayTask = Task.Delay(milliseconds);
                delayTask.Wait();
            }
        }

        public static void Delay(this DispatcherObject dispatcherObject, Action asyncAction, Action syncAction)
        {
            Task.Run((Action)AsyncAction);

            void AsyncAction()
            {
                asyncAction?.Invoke();
                dispatcherObject.Dispatcher.BeginInvoke((Action)SyncAction);
            }

            void SyncAction()
            {
                syncAction?.Invoke();
            }
        }

        public static T Convert<T>(string value, Func<string, T> func)
        {
            if (string.IsNullOrWhiteSpace(value)) return default;
            try
            {
                return func(value);
            }
            catch
            {
                return default;
            }
        }

        public static Brush ConvertToBrush(string color)
        {
            return Convert(color, (e) =>
            {
                var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(e));
                brush.Freeze();
                return brush;
            });
        }

        public static string AssemblyFolder()
            => Path.GetDirectoryName(ExecutingAssembly());

        public static string ExecutingAssembly()
            => Assembly.GetExecutingAssembly().GetName().CodeBase.Substring(8).Replace('/', '\\');

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
    }
}