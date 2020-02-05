using Docdown.Properties;
using Docdown.ViewModel;
using Docdown.Windows;
using System;
using System.Windows;

namespace Docdown.Util
{
    public static class MessageBox
    {
        public static MessageBoxResult Show(string title, string message, MessageBoxButton messageBoxButton)
        {
            return Show(title, message, messageBoxButton, null);
        }

        public static MessageBoxResult Show(string title, string message, MessageBoxButton messageBoxButton, string savedKey)
        {
            ReflectionUtility.EnsureMainThread();

            MessageBoxResult? savedResult = null;

            if (savedKey != null)
            {
                var savedValue = Settings.Default[savedKey];
                if (savedValue != null)
                {
                    Enum.TryParse(savedValue.ToString(), out MessageBoxResult result);
                    savedResult = result;
                }
            }

            if (savedResult != null)
            {
                return savedResult.Value;
            }

            var viewModel = new MessageBoxViewModel(title, message, messageBoxButton, savedKey != null);
            MessageWindow messageWindow = new MessageWindow(viewModel);
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
                if (savedKey != null)
                {
                    Settings.Default[savedKey] = viewModel.Result.ToString();
                    Settings.Default.Save();
                }
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
