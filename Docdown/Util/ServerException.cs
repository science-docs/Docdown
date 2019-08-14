using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Docdown.Util
{
    public class ServerException : Exception
    {
        public override string Message => message;

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
