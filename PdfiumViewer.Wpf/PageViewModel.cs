﻿using PdfiumViewer.Wpf.Util;
using System.Drawing;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

using WpfSize = System.Windows.Size;

namespace PdfiumViewer.Wpf
{
    internal class PageViewModel
    {
        public IPdfDocument Document { get; }
        public int Page { get; }
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

        public BitmapSource Render()
        {
            var image = Document.Render(Page, (int)RenderSize.Width, (int)RenderSize.Height, 
                (float)BitmapUtility.DpiX, (float)BitmapUtility.DpiY, PdfRenderFlags.Annotations);
            return BitmapUtility.ToBitmapSource(image);
        }

        private WpfSize ConvertSize(SizeF size, WpfSize availableSize, out WpfSize renderSize)
        {
            double ratio;
            if (Orientation == Orientation.Vertical)
            {
                ratio = size.Width / availableSize.Width;
            }
            else
            {
                ratio = size.Height / availableSize.Height;
            }
            var newSize = new WpfSize((int)(size.Width / ratio), (int)(size.Height / ratio));
            renderSize = BitmapUtility.ConvertSize(newSize);
            return newSize;
        }
    }
}