using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Docdown.Net
{
    public class ServerException : Exception
    {
        public override string Message => message;
        public string PandocMessage { get; private set; }

        private string message;

        public static async Task<ServerException> Create(HttpResponseMessage response)
        {
            var e = new ServerException();
            
            try
            {
                var text = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(text);
                var error = json.SelectToken("error");
                var message = error.SelectToken("message").Value<string>();
                var pandoc = (string)error.SelectToken("pandoc");

                e.PandocMessage = pandoc;
                e.message = message;
            }
            catch
            {
                e.message = response.ReasonPhrase;
            }

            return e;
        }
    }
}
