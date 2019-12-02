using Docdown.ViewModel;
using Docdown.Windows;
using System.Windows;

namespace Docdown.Util
{
    public static class MessageBox
    {
        public static MessageBoxResult Show(string title, string message, MessageBoxButton messageBoxButton)
        {
            ReflectionUtility.EnsureMainThread();
            var viewModel = new MessageBoxViewModel(title, message, messageBoxButton);
            MessageWindow messageWindow = new MessageWindow
            {
                DataContext = viewModel
            };
            if (Application.Current.MainWindow.IsVisible)
            {
                messageWindow.Owner = Application.Current.MainWindow;
            }
            else
            {
                messageWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
            if (messageWindow.ShowDialog().HasValue)
            {
                return viewModel.Result;
            }
            else if (messageBoxButton == MessageBoxButton.YesNo)
            {
                return MessageBoxResult.No;
            }
            else
            {
                return MessageBoxResult.Cancel;
            }
        }
    }
}
