﻿using PdfiumViewer.Wpf.Util;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace PdfiumViewer.Wpf
{
    public class PdfViewer : Control
    {
        public static readonly DependencyProperty MaxScaleProperty;
        public static readonly DependencyProperty DocumentProperty;
        public static readonly DependencyProperty DocumentPathProperty;

        static PdfViewer()
        {
            var type = typeof(PdfViewer);

            MaxScaleProperty = DependencyProperty.Register(nameof(MaxScale),
                typeof(double), type, new PropertyMetadata(double.NaN, OnMaxScaleChanged));
            DocumentProperty = DependencyProperty.Register(nameof(Document), 
                typeof(IPdfDocument), type, new PropertyMetadata(null, OnDocumentChanged));
            DocumentPathProperty = DependencyProperty.Register(nameof(DocumentPath),
                typeof(string), type, new PropertyMetadata(null, OnDocumentPathChanged));

            StackPanel.OrientationProperty.AddOwner(type, 
                new FrameworkPropertyMetadata(
                    Orientation.Vertical,
                    FrameworkPropertyMetadataOptions.AffectsMeasure,
                    OnOrientationChanged));
        }

        public double MaxScale
        {
            get => (double)GetValue(MaxScaleProperty);
            set => SetValue(MaxScaleProperty, value);
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
            Template = ResourceUtility.TryFindResource<ControlTemplate>("PdfViewerTemplate");
        }

        private void DisplayPdf(IPdfDocument document)
        {
            var itemsControl = this.GetDescendantByType<ItemsControl>();
            pageCache = new PageViewModel[document.PageCount];
            for (int i = 0; i < pageCache.Length; i++)
            {
                pageCache[i] = new PageViewModel(document, i)
                {
                    Orientation = Orientation,
                    MaxScale = MaxScale
                };
            }
            itemsControl.ItemsSource = pageCache;
        }

        private static void OnDocumentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is IPdfDocument document && d is PdfViewer viewer)
            {
                viewer.DisplayPdf(document);
            }
        }

        private static void OnDocumentPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PdfViewer pdfViewer)
            {
                var path = e.NewValue as string;
                IPdfDocument doc = !string.IsNullOrWhiteSpace(path) && File.Exists(path) 
                                        ? PdfDocument.Load(path) : null;
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

        private static void OnMaxScaleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PdfViewer pdfViewer)
            {
                var maxScale = (double)e.NewValue;
                for (int i = 0; i < pdfViewer.pageCache.Length; i++)
                {
                    pdfViewer.pageCache[i].MaxScale = maxScale;
                }
            }
        }
    }
}
