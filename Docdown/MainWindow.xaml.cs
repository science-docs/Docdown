using Docdown.ViewModel;
using Docdown.Windows;

namespace Docdown
{
    public partial class MainWindow
    {
        WorkspaceViewModel workspaceViewModel;

        public MainWindow()
        {
            var splash = new SplashWindow();
            if (splash.ShowDialog().Value)
            {
                InitializeComponent();
                DataContext = workspaceViewModel = splash.ViewModel.Data;
            }
            else
            {
                Close();
            }
        }

        private void MenuItemExitClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            Close();
        }
    }
}