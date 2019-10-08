using Docdown.Util;
using Docdown.ViewModel;
using Docdown.Windows;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;

namespace Docdown
{
    public partial class MainWindow
    {
        readonly AppViewModel app;
        bool closeFlag = true;

        public MainWindow()
        {
            //if (Debugger.IsAttached)
            //    Settings.Default.Reset();

            var splash = new SplashWindow();
            if (splash.ShowDialog().Value)
            {
                InitializeComponent();
                DataContext = app = splash.ViewModel.Data;
                Closing += OnClosing;
            }
            else
            {
                Close();
            }
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = closeFlag;
            if (closeFlag)
            {
                var newArgs = new CancelEventArgs();
                Task.Run(async () =>
                {
                    await app.OnClosing(newArgs);
                    if (!newArgs.Cancel)
                    {
                        closeFlag = false;
                        Dispatcher.Invoke(Close);
                    }
                });
            }
        }

        private void MenuItemExitClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MarkdownHelpMenuClicked(object sender, RoutedEventArgs e)
        {
            ProgramUtility.ExecuteNonWaiting("https://pandoc.org/MANUAL.html#pandocs-markdown");
        }

        private void AboutHelpMenuClicked(object sender, RoutedEventArgs e)
        {
            ProgramUtility.ExecuteNonWaiting("https://github.com/Darkgaja/Docdown");
        }
    }
}