using PdfiumViewer.Wpf.Util;
using System.Windows;
using System.Windows.Controls;

namespace PdfiumViewer.Wpf
{
    public class PdfViewer : Control
    {
        public static readonly DependencyProperty DocumentProperty;
        public static readonly DependencyProperty DocumentPathProperty;

        static PdfViewer()
        {
            var type = typeof(PdfViewer);
            
            DocumentProperty = DependencyProperty.Register(nameof(Document), 
                typeof(IPdfDocument), type, new PropertyMetadata(null, DocumentChanged));
            DocumentPathProperty = DependencyProperty.Register(nameof(DocumentPath),
                typeof(string), type, new PropertyMetadata(null, DocumentPathChanged));

            StackPanel.OrientationProperty.AddOwner(type, 
                new FrameworkPropertyMetadata(
                    Orientation.Vertical,
                    FrameworkPropertyMetadataOptions.AffectsMeasure,
                    OnOrientationChanged));
        }
        
        public IPdfDocument Document
        {
            get => (IPdfDocument)GetValue(DocumentProperty);
            set => SetValue(DocumentProperty, value);
        }

        public string DocumentPath
        {
            get => (string)GetValue(DocumentPathProperty);
            set => SetValue(DocumentPathProperty, value);
        }

        public Orientation Orientation
        {
            get => (Orientation)GetValue(StackPanel.OrientationProperty);
            set => SetValue(StackPanel.OrientationProperty, value);
        }

        private PageViewModel[] pageCache = new PageViewModel[0];

        public PdfViewer()
        {
            Style = ResourceUtility.TryFindResource<Style>("PdfViewerStyle");
        }

        private static void DocumentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is IPdfDocument document && d is PdfViewer viewer)
            {
                viewer.DisplayPdf(document);
            }
        }

        private void DisplayPdf(IPdfDocument document)
        {
            var itemsControl = this.GetDescendantByType<ItemsControl>();
            pageCache = new PageViewModel[document.PageCount];
            for (int i = 0; i < pageCache.Length; i++)
            {
                pageCache[i] = new PageViewModel(document, i)
                {
                    Orientation = Orientation
                };
            }
            itemsControl.ItemsSource = pageCache;
        }

        private static void DocumentPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PdfViewer pdfViewer)
            {
                var path = e.NewValue as string;
                IPdfDocument doc = PdfDocument.Load(path);
                pdfViewer.Document = doc;
            }
        }

        private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PdfViewer pdfViewer)
            {
                var orientation = (Orientation)e.NewValue;
                for (int i = 0; i < pdfViewer.pageCache.Length; i++)
                {
                    pdfViewer.pageCache[i].Orientation = orientation;
                }
            }
        }
    }
}
