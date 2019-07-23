using Docdown.Util;
using System;
using System.Windows;

namespace Docdown.ViewModel.Commands
{
    public class OpenWindowCommand<T> : DelegateCommand<bool?> where T : Window, new()
    {
        private static readonly TypeCache<T> windows = new TypeCache<T>();

        public OpenWindowCommand() : this(null, null)
        {
        }

        public OpenWindowCommand(object dataContext) : this(dataContext, null)
        {
        }

        public OpenWindowCommand(object dataContext, Action<bool?> callback) : base(dataContext, callback)
        {
        }

        [Delegate]
        [STAThread]
        private static bool? OpenWindow(object dataContext, Action<bool?> callback, bool keep = false)
        {
            if (!windows.TryGetValue(typeof(T), out var window))
            {
                window = new T
                {
                    Owner = Application.Current.MainWindow
                };
                if (dataContext != null)
                {
                    window.DataContext = dataContext;
                }
                if (keep)
                {
                    windows.Add(typeof(T), window);
                    window.Closing += KeptWindowClosing;
                }
            }
            var result = window.ShowDialog();
            callback?.Invoke(result);
            return result;
        }

        private static void KeptWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            (sender as Window).Hide();
        }
    }
}