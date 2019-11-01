using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Docdown.Util
{
    public static class DependencyPropertyUtility
    {
        public static readonly DependencyProperty CloseCommandProperty
            = DependencyProperty.RegisterAttached("CloseCommand", typeof(ICommand), typeof(DependencyPropertyUtility));

        public static readonly DependencyProperty FullSelectProperty
            = DependencyProperty.RegisterAttached("FullSelect", typeof(bool), typeof(DependencyPropertyUtility), new PropertyMetadata(false, FullSelectChanged));
        public static void SetCloseCommand(DependencyObject dependencyObject, ICommand value)
        {
            dependencyObject.SetValue(CloseCommandProperty, value);
        }

        public static ICommand GetCloseCommand(DependencyObject dependencyObject)
        {
            return (ICommand)dependencyObject.GetValue(CloseCommandProperty);
        }

        public static void SetFullSelect(DependencyObject dependencyObject, bool value)
        {
            dependencyObject.SetValue(FullSelectProperty, value);
        }

        public static bool GetFullSelect(DependencyObject dependencyObject)
        {
            return (bool)dependencyObject.GetValue(FullSelectProperty);
        }

        private static void FullSelectChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is TextBoxBase textBox && e.NewValue is bool value && value)
            {
                textBox.SelectAll();
                FocusManager.SetFocusedElement(Application.Current.MainWindow, textBox);
            }
        }
    }
}
