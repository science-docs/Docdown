using System.Windows;

namespace Docdown.Windows
{
    public partial class SettingsWindow
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void CloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}