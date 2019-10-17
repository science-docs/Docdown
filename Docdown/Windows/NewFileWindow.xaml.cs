using System.Windows;

namespace Docdown.Windows
{
    public partial class NewFileWindow
    {
        public NewFileWindow()
        {
            InitializeComponent();
        }

        private void OKClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CloseClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
