using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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
            get;
            set;
        }

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
    }
}
