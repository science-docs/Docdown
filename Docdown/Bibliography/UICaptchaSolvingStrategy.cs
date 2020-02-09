using System.Threading.Tasks;
using System.Windows;
using Docdown.ViewModel;
using Docdown.Windows;

namespace Docdown.Bibliography
{
    public class UICaptchaSolvingStrategy : ICaptchaSolvingStrategy
    {
        public async Task<string> Solve(byte[] imageData)
        {
            return await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var window = new CaptchaWindow();
                var vm = new CaptchaViewModel();
                vm.LoadByteArray(imageData);
                window.DataContext = vm;
                if (window.ShowDialog().Value)
                {
                    return vm.Captcha;
                }
                return null;
            });
            
        }
    }
}
