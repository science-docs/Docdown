using Docdown.Util;
using Docdown.ViewModel;
using PdfiumViewer;
using PdfiumViewer.Wpf.Util;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Animation;

namespace Docdown.Controls
{
    public partial class DocumentViewer
    {
        public DocumentViewer()
        {
            //this.AddHandler(nameof(WorkspaceItemViewModel.PdfPath), PdfPathChanged);
            //this.AddHandler(nameof(WorkspaceItemViewModel.IsConverting), ConvertingChanged);
            InitializeComponent();
        }
        
        public void Navigate(string fileName)
        {
            if (!File.Exists(fileName))
            {
                throw new IOException(nameof(fileName));
            }
            var doc = PdfDocument.Load(fileName);

            Dispatcher.BeginInvoke((Action)(() =>
            {
                Viewer.Document = doc;
                Viewer.Opacity = 1;
                Viewer.Visibility = Visibility.Visible;
            }));
        }
        
        private void PdfPathChanged()
        {
            if (DataContext is WorkspaceItemViewModel item)
            {
                try
                {
                    Navigate(item.PdfPath);
                }
                catch (Exception e)
                {
                    item.ErrorMessage = ErrorUtility.GetErrorMessage(e);
                }
            }
        }

        private void ConvertingChanged()
        {
            if (DataContext is WorkspaceItemViewModel item)
            {
                if (item.IsConverting)
                {
                    Viewer.BeginStoryboard(ResourceUtility.TryFindResource<Storyboard>("DecreaseOpacityAnimation"));
                    Spinner.BeginStoryboard(ResourceUtility.TryFindResource<Storyboard>("StartSpinnerAnimation"));
                }
                else
                {
                    Viewer.BeginStoryboard(ResourceUtility.TryFindResource<Storyboard>("IncreaseOpacityAnimation"));
                }
            }
        }
    }
}