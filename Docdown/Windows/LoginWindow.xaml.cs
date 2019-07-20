using Docdown.ViewModel;
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
    public partial class LoginWindow
    {
        public LoginWindow()
        {
            var login = new LoginViewModel();
            DataContextChanged += (o, e) =>
            {
                var log = e.NewValue as LoginViewModel;
                log.CloseRequested += (_, __) => Close();
            };
            DataContext = login;
            InitializeComponent();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext != null)
                ((dynamic)DataContext).SecurePassword = ((PasswordBox)sender).SecurePassword;
        }
    }
}
