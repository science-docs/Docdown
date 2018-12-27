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
            this.AddHandler(nameof(WorkspaceViewModel.SelectedPdfPath), PdfPathChanged);
            this.AddHandler(nameof(WorkspaceViewModel.IsConverting), ConvertingChanged);
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
            if (DataContext is WorkspaceViewModel workspace)
            {
                Navigate(workspace.SelectedPdfPath);
            }
        }

        private void ConvertingChanged()
        {
            if (DataContext is WorkspaceViewModel workspace)
            {
                if (workspace.IsConverting)
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