using Docdown.ViewModel;
using System;

namespace Docdown.Windows
{
    public partial class SplashWindow
    {
        public SplashViewModel ViewModel { get; }

        private bool run = false;

        public SplashWindow()
        {
            InitializeComponent();
            DataContext = ViewModel = new SplashViewModel(SetDialogResult);
        }

        private void SplashInitialized(object sender, EventArgs e)
        {
            if (!run)
            {
                run = true;
                ViewModel.Initialize();
            }
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