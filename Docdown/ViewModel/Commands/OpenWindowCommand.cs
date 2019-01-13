using System;
using System.Windows;

namespace Docdown.ViewModel.Commands
{
    public class OpenWindowCommand<T> : DelegateCommand where T : Window, new()
    {
        public OpenWindowCommand() : this(null, null)
        {
        }

        public OpenWindowCommand(object dataContext) : this(dataContext, null)
        {
        }

        public OpenWindowCommand(object dataContext, Action<bool?> callback = null) : base(dataContext, callback)
        {
        }

        [Delegate]
        private static bool? OpenWindow(object dataContext, Action<bool?> callback)
        {
            var window = new T
            {
                Owner = Application.Current.MainWindow,
                DataContext = dataContext
            };
            var result = window.ShowDialog();
            callback?.Invoke(result);
            return result;
        }
    }
}