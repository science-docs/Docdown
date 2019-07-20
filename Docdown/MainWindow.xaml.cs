using Docdown.Properties;
using Docdown.Util;
using Docdown.ViewModel;
using Docdown.Windows;
using System.Diagnostics;

namespace Docdown
{
    public partial class MainWindow
    {
        readonly AppViewModel app;

        public MainWindow()
        {
            if (Debugger.IsAttached)
                Settings.Default.Reset();

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

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            app.OnClosing(e);
        }

        private void MenuItemExitClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            Close();
        }

        private void MarkdownHelpMenuClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            ProgramUtility.Execute("https://pandoc.org/MANUAL.html#pandocs-markdown");
        }

        private void AboutHelpMenuClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            ProgramUtility.Execute("https://github.com/Darkgaja/Docdown");
        }
    }
}