using System.Windows;

namespace Docdown.Installer
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Extractor_Click(object sender, RoutedEventArgs e)
        {
            const string name = "Docdown.Installer.Resources.Docdown.exe";
            const string output = "./Docdown.exe";

             await Util.Extract(name, output);
        }
    }
}
