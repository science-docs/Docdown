using System.Windows;

namespace Docdown.Windows
{
    public partial class WizardWindow
    {
        public WizardWindow()
        {
            InitializeComponent();
        }

        private void CloseButtonClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}