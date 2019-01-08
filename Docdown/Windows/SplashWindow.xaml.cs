using Docdown.ViewModel;
using System;

namespace Docdown.Windows
{
    public partial class SplashWindow
    {
        public SplashViewModel ViewModel { get; }

        public SplashWindow()
        {
            InitializeComponent();
            DataContext = ViewModel = new SplashViewModel(SetDialogResult);
        }

        private void SplashInitialized(object sender, EventArgs e)
        {
            ViewModel.Initialize();
        }

        private void SetDialogResult(bool? dialogResult)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                DialogResult = dialogResult;
            }));
        }
    }
}