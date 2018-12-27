using System;
using System.Net;

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
    }
}
