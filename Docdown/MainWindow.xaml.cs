using Docdown.Properties;
using Docdown.Util;
using Docdown.ViewModel;
using Docdown.Windows;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

namespace Docdown
{
    public partial class MainWindow
    {
        readonly AppViewModel app;

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
            e.Cancel = true;
            var newArgs = new CancelEventArgs();
            Task.Run(async () =>
            {
                await app.OnClosing(newArgs);
                if (!newArgs.Cancel)
                {
                    Environment.Exit(0);
                }
            });
        }

        private void MenuItemExitClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MarkdownHelpMenuClicked(object sender, RoutedEventArgs e)
        {
            ProgramUtility.Execute("https://pandoc.org/MANUAL.html#pandocs-markdown");
        }

        private void AboutHelpMenuClicked(object sender, RoutedEventArgs e)
        {
            ProgramUtility.Execute("https://github.com/Darkgaja/Docdown");
        }
    }
}