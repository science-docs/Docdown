using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using System.Windows.Media;

namespace PdfiumViewer.Wpf
{
    public class PdfViewer : ContentControl
    {
        public static readonly DependencyProperty ShowToolbarProperty;
        public static readonly DependencyProperty ShowBookmarksProperty;
        public static readonly DependencyProperty DocumentProperty;
        public static readonly DependencyProperty ZoomModeProperty;

        static PdfViewer()
        {
            var type = typeof(PdfViewer);

            ShowToolbarProperty = DependencyProperty.Register(nameof(ShowToolbar), typeof(bool), type, new PropertyMetadata(true, ShowToolbarChanged));
            ShowBookmarksProperty = DependencyProperty.Register(nameof(ShowBookmarks), typeof(bool), type, new PropertyMetadata(true, ShowBookmarksChanged));
            DocumentProperty = DependencyProperty.Register(nameof(Document), typeof(IPdfDocument), type, new PropertyMetadata(null, DocumentChanged));
            ZoomModeProperty = DependencyProperty.Register(nameof(ZoomMode), typeof(PdfViewerZoomMode), type, new PropertyMetadata(PdfViewerZoomMode.FitBest, ZoomModeChanged));

            Panel.BackgroundProperty.AddOwner(type, new FrameworkPropertyMetadata(Brushes.White, BackgroundChanged));
        }

        public bool ShowToolbar
        {
            get => (bool)GetValue(ShowToolbarProperty);
            set => SetValue(ShowToolbarProperty, value);
        }

        public bool ShowBookmarks
        {
            get => (bool)GetValue(ShowBookmarksProperty);
            set => SetValue(ShowBookmarksProperty, value);
        }

        public IPdfDocument Document
        {
            get => (IPdfDocument)GetValue(DocumentProperty);
            set => SetValue(DocumentProperty, value);
        }

        public PdfViewerZoomMode ZoomMode
        {
            get => (PdfViewerZoomMode)GetValue(ZoomModeProperty);
            set => SetValue(ZoomModeProperty, value);
        }

        public PdfRenderer Renderer => viewer.Renderer;

        public event LinkClickEventHandler LinkClick
        {
            add
            {
                viewer.LinkClick += value;
            }
            remove
            {
                viewer.LinkClick -= value;
            }
        }

        private readonly WindowsFormsHost host;
        private readonly PdfiumViewer.PdfViewer viewer;

        public PdfViewer()
        {
            Content = host = new WindowsFormsHost();
            host.Child = viewer = new PdfiumViewer.PdfViewer();
        }

        private static void ShowToolbarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PdfViewer viewer) viewer.viewer.ShowToolbar = (bool)e.NewValue;
        }

        private static void ShowBookmarksChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PdfViewer viewer) viewer.viewer.ShowBookmarks = (bool)e.NewValue;
        }

        private static void DocumentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null && d is PdfViewer viewer) viewer.viewer.Document = (IPdfDocument)e.NewValue;
        }

        private static void ZoomModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PdfViewer viewer) viewer.viewer.ZoomMode = (PdfViewerZoomMode)e.NewValue;
        }

        private static void BackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PdfViewer viewer)
            {
                switch (e.NewValue)
                {
                    case SolidColorBrush solidColorBrush:
                        var color = solidColorBrush.Color;
                        var winColor = System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
                        viewer.Renderer.BackColor = winColor;
                        break;
                    default:
                        viewer.Renderer.BackColor = System.Drawing.Color.Empty;
                        break;
                }
            }
        }
    }
}