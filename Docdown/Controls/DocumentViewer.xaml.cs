using PdfiumViewer;
using System;
using System.IO;
using System.Windows;

namespace Docdown.Controls
{
    public partial class DocumentViewer
    {
        public DocumentViewer()
        {
            InitializeComponent();
        }

        public void Navigate(string fileName)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                Error.Visibility = Visibility.Collapsed;
                if (!File.Exists(fileName))
                {
                    ShowError("File does not exist");
                    return;
                }

                try
                {

                    var doc = PdfDocument.Load(fileName);
                    EndLoad();
                    Viewer.Document = doc;
                    Viewer.Opacity = 1;
                    Viewer.Visibility = Visibility.Visible;
                }
                catch
                {
                    ShowError("Could not load PDF file");
                }
            }));
        }

        public void ShowError(string message)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                EndLoad();
                Viewer.Visibility = Visibility.Collapsed;
                Error.Visibility = Visibility.Visible;
                Error.Text = message;
            }));
        }

        public void DismissError()
        {
            Error.Visibility = Visibility.Collapsed;
        }

        public void StartLoad()
        {
            Viewer.Opacity = 0.3;
            Spinner.Visibility = Visibility.Visible;
        }

        public void EndLoad()
        {
            Spinner.Visibility = Visibility.Collapsed;
        }

    }
}