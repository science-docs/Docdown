using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Docdown.Windows
{
    public partial class InputWindow
    {
        public InputWindow()
        {
            InitializeComponent();
        }

        private void CloseClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OkClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
