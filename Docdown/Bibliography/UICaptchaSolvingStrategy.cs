using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Docdown.ViewModel;
using Docdown.Windows;

namespace Docdown.Bibliography
{
    public class UICaptchaSolvingStrategy : ICaptchaSolvingStrategy
    {
        private CaptchaWindow window;
        private CaptchaViewModel viewModel;
        private readonly SemaphoreSlim signal = new SemaphoreSlim(0);

        public async Task Finish()
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                window.Close();
            });
        }

        public async Task Initialize()
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                window = new CaptchaWindow
                {
                    Owner = Application.Current.MainWindow
                };
                window.Closing += WindowClosing;
                viewModel = new CaptchaViewModel();
                viewModel.CaptchaEntered += CaptchaEntered;
                window.DataContext = viewModel;
            });
        }

        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            signal.Release();
        }

        private void CaptchaEntered(object sender, EventArgs e)
        {
            signal.Release();
        }

        public async Task<CaptchaResult> Solve(byte[] imageData)
        {
            if (window == null || viewModel == null)
                throw new InvalidOperationException("Initialize the solving strategy first");

            viewModel.LoadByteArray(imageData);

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                if (!window.IsVisible)
                {
                    window.Show();
                }
            });

            await signal.WaitAsync();

            if (!window.IsVisible)
            {
                return CaptchaResult.AbortedResult;
            }

            return viewModel.Result;
        }

        public Task Invalidate(string lastCaptcha)
        {
            viewModel.Invalidate(lastCaptcha);
            return Task.CompletedTask;
        }
    }
}
