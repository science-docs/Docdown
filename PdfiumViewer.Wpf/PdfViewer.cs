using PdfiumViewer.Wpf.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace PdfiumViewer.Wpf
{
    public class PdfViewer : Control
    {
        public static readonly DependencyProperty DocumentProperty;

        static PdfViewer()
        {
            var type = typeof(PdfViewer);
            
            DocumentProperty = DependencyProperty.Register(nameof(Document), typeof(IPdfDocument), type, new PropertyMetadata(null, DocumentChanged));
        }

        public IPdfDocument Document
        {
            get => (IPdfDocument)GetValue(DocumentProperty);
            set => SetValue(DocumentProperty, value);
        }

        public PdfViewer()
        {
            Style = ResourceUtility.TryFindResource<Style>("PdfViewerStyle");
        }

        private static void DocumentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is IPdfDocument document && d is PdfViewer viewer)
            {
                var reference = viewer.GetDescendantByName<System.Windows.Shapes.Rectangle>("Reference");
                var width = reference.ActualWidth;
                
                Task.Run(() =>
                {
                    var watch = Stopwatch.StartNew();
                    var bitmaps = viewer.RenderPdf(document, (int)width);
                    Debug.WriteLine("Converted in: " + watch.ElapsedMilliseconds);
                    viewer.DisplayPdf(bitmaps);
                });
            }
        }

        private BitmapSource[] RenderPdf(IPdfDocument document, int width)
        {
            var images = new BitmapSource[document.PageCount];

            for (int i = 0; i < document.PageCount; i++)
            {
                var size = ConvertSize(document.PageSizes[i], width);
                var image = document.Render(i, (int)size.Width, (int)size.Height, BitmapUtility.DpiX, BitmapUtility.DpiY, PdfRenderFlags.Annotations);
                images[i] = BitmapUtility.ToBitmapSource(image);
            }

            return images;
        }

        private SizeF ConvertSize(SizeF size, int width)
        {
            var ratio = size.Width / width;
            return new SizeF(size.Width / ratio, size.Height / ratio);
        }

        private void DisplayPdf(BitmapSource[] bitmaps)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                var itemsControl = this.GetDescendantByType<ItemsControl>();
                itemsControl.ItemsSource = bitmaps;
            }));
        }
    }
}
