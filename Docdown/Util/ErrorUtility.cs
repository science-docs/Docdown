using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Navigation;

namespace Docdown.Util
{
    public static class ErrorUtility
    {
        public static async Task<Inline> GetErrorMessage(Exception exception)
        {
            return await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                switch (exception)
                {
                    case WebException webException:
                        return new Run(HandleWebException(webException));
                    case ServerException serverException:
                        return HandleServerException(serverException);
                }
                return new Run(exception.Message);
            });
        }

        private static Inline HandleServerException(ServerException exception)
        {
            if (exception.PandocMessage == null)
            {
                return new Run(exception.Message);
            }
            else
            {
                var span = new Span();
                var message = exception.Message.Trim().TrimEnd('.') + ". Click ";
                span.Inlines.Add(message);
                var link = new Hyperlink(new Run("here"))
                {
                    Tag = exception.PandocMessage
                };
                link.NavigateUri = new Uri("Opens a message box", UriKind.Relative);
                link.RequestNavigate += HandleExceptionLinkClick;
                span.Inlines.Add(link);
                span.Inlines.Add(" for more information.");
                return span;
            }
        }

        private static void HandleExceptionLinkClick(object sender, RequestNavigateEventArgs e)
        {
            var link = (Hyperlink)sender;
            var message = (string)link.Tag;
            MessageBox.Show("Error Information", message, MessageBoxButton.OK);
            e.Handled = true;
        }

        private static string HandleWebException(WebException exception)
        {
            switch (exception.Status)
            {
                case WebExceptionStatus.ConnectFailure:
                    return "Could not connect to server";
            }
            if (exception.Response is HttpWebResponse response)
            {
                return response.StatusDescription;
            }
            return exception.Message;
        }

        public static void WriteLog(Exception e)
        {
            StringBuilder sb = new StringBuilder("Exception: " + e.GetType());
            sb.AppendLine();
            sb.AppendLine(e.Message);
            sb.AppendLine(e.StackTrace);
            while (e.InnerException != null)
            {
                e = e.InnerException;
                sb.AppendLine("Inner Exception: " + e.GetType());
                sb.AppendLine(e.Message);
                sb.AppendLine(e.StackTrace);
            }
            File.AppendAllText("log.txt", sb.ToString());
        }
    }
}
