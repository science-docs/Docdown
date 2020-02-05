using HtmlAgilityPack;
using System.Reflection;
using System.Windows.Controls;

namespace Docdown.Controls
{
    public partial class WebViewer
    {
        public string AfterNavigation { get; set; }

        public WebViewer()
        {
            InitializeComponent();
        }

        public void Navigate(string url)
        {
            Dispatcher.Invoke(() =>
            {
                Viewer.Navigate(url);
            });
        }

        public void Navigate(HtmlDocument htmlDocument)
        {
            Dispatcher.Invoke(() =>
            {
                Viewer.NavigateToString(htmlDocument.Text);
            });
        }

        private void ViewerUnloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            // This disposes all content of the web browser
            Viewer.Source = null;
        }
    }
}
