using System;
using System.IO;
using System.Net;
using System.Text;

namespace Docdown.Util
{
    public static class ErrorUtility
    {
        public static string GetErrorMessage(Exception exception)
        {
            switch (exception)
            {
                case WebException webException:
                    return HandleWebException(webException);
            }
            return exception.Message;
        }

        public static string HandleWebException(WebException exception)
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
