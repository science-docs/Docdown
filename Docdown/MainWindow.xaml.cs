using Docdown.Util;
using Docdown.ViewModel;
using Docdown.Windows;

namespace Docdown
{
    public partial class MainWindow
    {
        readonly WorkspaceViewModel workspaceViewModel;

        public MainWindow()
        {
            var splash = new SplashWindow();
            if (splash.ShowDialog().Value)
            {
                InitializeComponent();
                DataContext = workspaceViewModel = splash.ViewModel.Data;
                Closing += OnClosing;
            }
            else
            {
                Close();
            }
        }

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            workspaceViewModel.OnClosing(e);
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