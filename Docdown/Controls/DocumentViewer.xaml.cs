using PdfiumViewer;
using System;

namespace Docdown.Controls
{
    public partial class DocumentViewer
    {
        public DocumentViewer()
        {
            InitializeComponent();
        }

        public void NavigateFile(string fileName)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                var doc = PdfDocument.Load(fileName);
                Viewer.Document = doc;
            }));
        }
    }
}