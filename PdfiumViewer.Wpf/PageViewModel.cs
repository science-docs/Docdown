using PdfiumViewer.Wpf.Util;
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
        public IPdfDocument Document { get; }
        public int Page { get; }
        public double MaxScale { get; set; }
        public WpfSize Size { get; private set; }
        public WpfSize RenderSize { get; private set; }
        public Orientation Orientation { get; set; }

        public PageViewModel(IPdfDocument document, int page)
        {
            Document = document;
            Page = page;
        }

        public WpfSize Resize(WpfSize availableSize)
        {
            Size = ConvertSize(Document.PageSizes[Page], availableSize, out var renderSize);
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
                    return Document.Render(Page, (int)RenderSize.Width, (int)RenderSize.Height,
                        (float)BitmapUtility.DpiX, (float)BitmapUtility.DpiY, PdfRenderFlags.Annotations);
                }
                catch
                {
                    return null;
                }
            });
        }
    }
}