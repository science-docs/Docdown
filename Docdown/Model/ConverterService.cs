using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Docdown.Util;
using Newtonsoft.Json.Linq;

namespace Docdown.Model
{
    public class ConverterService : IConverterService
    {
        public string Url { get; set; }

        public async Task<ConnectionStatus> Connect()
        {
            try
            {
                await WebUtility.SimpleTextRequest(Url);
                return ConnectionStatus.Connected;
            }
            catch
            {
                return ConnectionStatus.Disconnected;
            }
        }

        public async Task<byte[]> Convert(IEnumerable<MultipartFormParameter> parameters, CancelToken cancelToken)
        {
            var token = cancelToken?.ToCancellationToken() ?? new CancellationToken();
            using (var req = await WebUtility.PostRequest(BuildConvertUrl(), parameters, token))
            {
                using (var ms = new MemoryStream())
                {
                    await req.Content.CopyToAsync(ms);
                    return ms.ToArray();
                }
            }
        }

        public async Task<string[]> LoadCsls()
        {
            var json = await WebUtility.SimpleJsonRequest(BuildCslUrl());
            return json
                    .Select(e => e.Value<string>())
                    .OrderBy(e => e)
                    .ToArray();
        }

        public async Task<Template[]> LoadTemplates()
        {
            var json = await WebUtility.SimpleJsonRequest(BuildTemplatesUrl());
            return Template.FromJson(json);
        }

        public static string BuildUrl(params string[] values)
        {
            return Path.Combine(values.Where(e => e != null).Select(e => e.Trim('/')).ToArray()).Replace('\\', '/');
        }

        private string BuildConvertUrl()
        {
            return BuildUrl(Url, "convert");
        }

        private string BuildTemplatesUrl()
        {
            return BuildUrl(Url, "templates");
        }

        private string BuildCslUrl()
        {
            return BuildUrl(Url, "csl");
        }
    }
}
