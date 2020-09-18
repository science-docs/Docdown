using PdfiumViewer.Wpf.Util;
using PdfiumLight;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

using WpfSize = System.Windows.Size;

namespace PdfiumViewer.Wpf
{
    internal class PageViewModel
    {
        public PdfDocument Document { get; }
        public int Page { get; }
        public double MaxScale { get; set; }
        public WpfSize Size { get; private set; }
        public WpfSize RenderSize { get; private set; }
        public Orientation Orientation { get; set; }

        public PageViewModel(PdfDocument document, int page)
        {
            Document = document;
            Page = page;
        }

        public WpfSize Resize(WpfSize availableSize)
        {
            Size = ConvertSize(Document.GetPageSize(Page), availableSize, out var renderSize);
            RenderSize = renderSize;
            return Size;
        }

        public async Task<BitmapSource> Render()
        {
            var image = await QueueRender();
            return BitmapUtility.ToBitmapSource(image);
        }

        private WpfSize ConvertSize(SizeF size, WpfSize availableSize, out WpfSize renderSize)
        {
            double ratio;
            if (Orientation == Orientation.Vertical)
            {
                ratio = availableSize.Width / size.Width;
            }
            else
            {
                ratio = availableSize.Height / size.Height;
            }
            if (!double.IsNaN(MaxScale))
            {
                ratio = Math.Min(ratio, MaxScale);
            }
            var newSize = new WpfSize((int)(size.Width * ratio), (int)(size.Height * ratio));
            renderSize = BitmapUtility.ConvertSize(newSize);
            return newSize;
        }
        
        private async Task<System.Drawing.Image> QueueRender()
        {
            return await TaskUtility.Enqueue(() =>
            {
                try
                {
                    var page = Document.GetPage(Page);
                    return page.Render((int)RenderSize.Width, (int)RenderSize.Height);
                }
                catch
                {
                    return null;
                }
            });
        }
    }
}