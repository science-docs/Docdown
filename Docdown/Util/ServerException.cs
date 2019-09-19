using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Docdown.Util
{
    public class ServerException : Exception
    {
        public override string Message => message;
        public string PandocMessage => pandoc;

        private string message;
        private string pandoc;

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

                e.pandoc = pandoc;
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
