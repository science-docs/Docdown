using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Docdown.Bibliography;
using Docdown.Model;
using Docdown.Validation;
using Docdown.ViewModel.Commands;

namespace Docdown.ViewModel
{
    public class CaptchaViewModel : ObservableObject
    {
        public ImageSource Image
        {
            get => image;
            set => Set(ref image, value);
        }

        public string Captcha
        {
            get => captcha;
            set
            {
                Set(ref captcha, value);
                StaticCaptchaValidationRule.Result = ValidationResult.ValidResult;
            }
        }

        public bool Reload { get; private set; }

        public CaptchaResult Result
        {
            get
            {
                if (Reload)
                {
                    Reload = false;
                    return CaptchaResult.ReloadResult;
                }
                else
                {
                    return new CaptchaResult(Captcha);
                }
            }
        }

        public ICommand ReloadCommand => new ActionCommand(() =>
        {
            Reload = true;
            CaptchaEntered?.Invoke(this, EventArgs.Empty);
        });

        public ICommand FinishCommand => new ActionCommand(() =>
        {
            Reload = false;
            CaptchaEntered?.Invoke(this, EventArgs.Empty);
        });

        public event EventHandler CaptchaEntered;

        private string captcha;
        private ImageSource image;

        public void LoadByteArray(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) 
                return;
            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            Image = image;
        }

        public void Invalidate(string lastCaptcha)
        {
            Captcha = string.Empty;
            StaticCaptchaValidationRule.Result = new ValidationResult(false, Language.Current.Get("Captcha.Error", lastCaptcha));
        }
    }
}
