using Docdown.ViewModel;
using System.Windows;

namespace Docdown.Windows
{
    public partial class MessageWindow
    {
        public MessageWindow(MessageBoxViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }

        private void CloseClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
