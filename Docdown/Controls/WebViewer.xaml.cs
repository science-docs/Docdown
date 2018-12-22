using HtmlAgilityPack;
using System;

namespace Docdown.Controls
{
    public partial class WebViewer
    {
        public WebViewer()
        {
            InitializeComponent();
        }

        public void Navigate(HtmlDocument htmlDocument)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                Viewer.NavigateToString(htmlDocument.Text);
            }));
        }
    }
}
