using System.Windows;

namespace Docdown.Windows
{
    public partial class MessageWindow
    {
        public MessageWindow()
        {
            InitializeComponent();
        }

        private void CloseClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
