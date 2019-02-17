using System.Windows;
using System.Windows.Input;

namespace Docdown.Util
{
    public static class DependencyPropertyUtility
    {
        public static readonly DependencyProperty CloseCommandProperty
            = DependencyProperty.RegisterAttached("CloseCommand", typeof(ICommand), typeof(DependencyPropertyUtility));

        public static void SetCloseCommand(DependencyObject dependencyObject, ICommand value)
        {
            dependencyObject.SetValue(CloseCommandProperty, value);
        }

        public static ICommand GetCloseCommand(DependencyObject dependencyObject)
        {
            return (ICommand)dependencyObject.GetValue(CloseCommandProperty);
        }
    }
}
