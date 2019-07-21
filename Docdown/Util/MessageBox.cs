using Docdown.ViewModel;
using Docdown.Windows;
using System;
using System.Windows;
using System.Windows.Threading;

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
                DataContext = viewModel,
                Owner = Application.Current.MainWindow
            };
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
