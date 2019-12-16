using Docdown.Model;
using Docdown.Properties;
using Docdown.Text;
using Docdown.Text.Markdown;
using Docdown.ViewModel;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Docdown.Util
{
    public static class WebUtility
    {
        public static ProductInfoHeaderValue UserAgent { get; } = new ProductInfoHeaderValue("Docdown", typeof(WebUtility).Assembly.GetName().Version.ToString());

        private static readonly WinHttpHandler handler = new WinHttpHandler();
        private static readonly HttpClient client = new HttpClient(handler);

        public static string BuildConvertUrl()
        {
            return BuildUrl(Settings.Default.API, "convert");
        }

        public static string BuildWorkspaceUrl()
        {
            return BuildUrl(Settings.Default.API, "workspace");
        }

        public static string BuildWorkspaceItemUrl()
        {
            return BuildUrl(Settings.Default.API, "workspace", "file");
        }

        public static string BuildWorkspaceConvertUrl()
        {
            return BuildUrl(Settings.Default.API, "workspace", "convert");
        }

        public static string BuildTemplatesUrl()
        {
            return BuildUrl(Settings.Default.API, "templates");
        }

        public static string BuildCslUrl()
        {
            return BuildUrl(Settings.Default.API, "csl");
        }

        public static string BuildRegisterUrl()
        {
            return BuildUrl(Settings.Default.API, "register");
        }

        public static string BuildLoginUrl()
        {
            return BuildUrl(Settings.Default.API, "login");
        }

        public static string BuildUrl(params string[] values)
        {
            return Path.Combine(values.Where(e => e != null).ToArray()).Replace('\\', '/');
        }

        public static async Task<HttpResponseMessage> DeleteRequest(string url, IEnumerable<MultipartFormParameter> postParameters)
        {
            return await SimpleRequest(url, HttpMethod.Delete, CancellationToken.None, postParameters.ToArray());
        }

        public static async Task<HttpResponseMessage> GetRequest(string url, IEnumerable<MultipartFormParameter> postParameters)
        {
            return await GetRequest(url, postParameters.ToArray());
        }

        public static async Task<byte[]> DownloadAsync(string url)
        {
            return await DownloadAsync(url, new Progress<WebDownloadProgress>());
        }

        public static async Task<byte[]> DownloadAsync(string url, IProgress<WebDownloadProgress> progress)
        {
            return await DownloadAsync(url, progress, CancellationToken.None);
        }

        public static async Task<byte[]> DownloadAsync(string url, IProgress<WebDownloadProgress> progress, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            
            using (var ms = new MemoryStream())
            {
                using (var stream = await response.Content.ReadAsStreamAsync())
                {
                    var clength = response.Content.Headers.ContentLength;
                    if (!clength.HasValue)
                    {
                        return new byte[0];
                    }
                    var totalLength = (int)clength.Value;
                    progress.Report(new WebDownloadProgress(totalLength, 0));
                    while (true)
                    {
                        var buffer = new byte[ushort.MaxValue]; // ushort.MaxValue is approximately 1% of docdowns size
                        var length = await stream.ReadAsync(buffer, 0, buffer.Length);

                        if (length == 0)
                        {
                            break;
                        }

                        await ms.WriteAsync(buffer, 0, length);
                        progress.Report(new WebDownloadProgress(totalLength, (int)ms.Length));
                    }
                }
                return ms.ToArray();
            }
        }

        public static async Task<HttpResponseMessage> SimpleRequest(string url, HttpMethod method, CancellationToken cancellationToken, IEnumerable<MultipartFormParameter> postParameters)
        {
            var request = new HttpRequestMessage
            {
                Method = method,
                RequestUri = new Uri(url)
            };
            request.Headers.UserAgent.Add(UserAgent);

            if (postParameters != null && postParameters.Any())
            {
                string formDataBoundary = $"----------{Guid.NewGuid()}";
                var content = new MultipartFormDataContent(formDataBoundary);
                foreach (var parameter in postParameters)
                {
                    if (parameter.FileName is null)
                    {
                        content.Add(parameter.ToHttpContent(), parameter.Name);
                    }
                    else
                    {
                        content.Add(parameter.ToHttpContent(), parameter.Name, parameter.FileName);
                    }
                }
                request.Content = content;
            }

            var response = await client.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var exception = await ServerException.Create(response);
                throw exception;
            }
            return response;
        }

        public static async Task<HttpResponseMessage> GetRequest(string url, params MultipartFormParameter[] postParameters)
        {
            return await SimpleRequest(url, HttpMethod.Get, CancellationToken.None, postParameters);
        }

        public static async Task<string> SimpleTextRequest(string url, params MultipartFormParameter[] postParameters)
        {
            using (var res = await GetRequest(url, postParameters))
            {
                return await res.Content.ReadAsStringAsync();
            }
        }

        public static async Task<JToken> SimpleJsonRequest(string url, params MultipartFormParameter[] parameters)
        {
            var text = await SimpleTextRequest(url, parameters);
            return JToken.Parse(text);
        }

        public static async Task<HttpResponseMessage> PostRequest(string postUrl, IEnumerable<MultipartFormParameter> postParameters)
        {
            return await PostRequest(postUrl, postParameters.ToArray());
        }

        private static readonly HttpMethod Move = new HttpMethod("MOVE");

        public static async Task<HttpResponseMessage> MoveRequest(string postUrl, IEnumerable<MultipartFormParameter> postParameters)
        {
            return await SimpleRequest(postUrl, Move, CancellationToken.None, postParameters.ToArray());
        }

        public static async Task<HttpResponseMessage> PostRequest(string postUrl, params MultipartFormParameter[] postParameters)
        {
            return await SimpleRequest(postUrl, HttpMethod.Post, CancellationToken.None, postParameters);
        }

        public static async Task<HttpResponseMessage> PostRequest(string postUrl, CancellationToken cancellationToken, IEnumerable<MultipartFormParameter> postParameters)
        {
            return await SimpleRequest(postUrl, HttpMethod.Post, cancellationToken, postParameters.ToArray());
        }
    }

    public enum MultipartFormParameterType : byte
    {
        Field = 0,
        File = 1
    }

    public class MultipartFormParameter
    {
        public string Name { get; set; }
        public byte[] Data { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public string[] Value { get; set; }
        public bool Array { get; set; } = false;
        public MultipartFormParameterType Type { get; set; }

        private const string BinaryType = "application/octet-stream";
        private const string MainFile = "content";

        private static readonly Dictionary<string, IPreprocessor> preprocessors = new Dictionary<string, IPreprocessor>();

        static MultipartFormParameter()
        {
            preprocessors[".md"] = new MarkdownPreprocessor();
        }
        private MultipartFormParameter() { }

        public HttpContent ToHttpContent()
        {
            if (Type == MultipartFormParameterType.File)
            {
                return new StreamContent(new MemoryStream(Data));
            }
            else
            {
                return new StringContent(Value[0]);
            }
        }
        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            if (obj is MultipartFormParameter parameter)
            {
                return Name == parameter.Name;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public static MultipartFormParameter CreateField(string name, string value)
        {
            if (name is null)
                throw new ArgumentNullException(nameof(name));
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            return new MultipartFormParameter
            {
                Name = name,
                Value = new[] { value },
                Type = MultipartFormParameterType.Field
            };
        }

        public static MultipartFormParameter CreateFieldArray(string name, string[] values)
        {
            if (name is null)
                throw new ArgumentNullException(nameof(name));
            if (values is null)
                throw new ArgumentNullException(nameof(values));

            return new MultipartFormParameter
            {
                Name = name,
                Value = values,
                Type = MultipartFormParameterType.Field
            };
        }

        public static MultipartFormParameter CreateFile(string name, string fileName)
        {
            return CreateFile(name, fileName, BinaryType);
        }

        public static MultipartFormParameter CreateFile(IWorkspaceItem item)
        {
            var bytes = item.Read();
            if (preprocessors.TryGetValue(item.FileInfo.Extension, out var preprocessor))
            {
                bytes = preprocessor.Preprocess(item, bytes);
            }
            return CreateFile(item.RelativeName, item.FullName, bytes);
        }

        public static MultipartFormParameter CreateFile(string name, string fileName, string contentType)
        {
            try
            {
                return CreateFile(name, fileName, File.ReadAllBytes(fileName), contentType);
            }
            catch
            {
                return null;
            }
        }

        public static MultipartFormParameter CreateFile(string name, string fileName, byte[] file)
        {
            return CreateFile(name, fileName, file, BinaryType);
        }

        public static MultipartFormParameter CreateFile(string name, string fileName, byte[] file, string contentType)
        {
            if (name is null)
                throw new ArgumentNullException(nameof(name));
            if (fileName is null)
                throw new ArgumentNullException(nameof(fileName));
            if (file is null)
                throw new ArgumentNullException(nameof(file));
            if (contentType is null)
                throw new ArgumentNullException(nameof(contentType));

            return new MultipartFormParameter
            {
                Name = name,
                Data = file,
                FileName = fileName,
                ContentType = contentType,
                Type = MultipartFormParameterType.File
            };
        }

        public static IEnumerable<MultipartFormParameter> ApiParameter(ConverterType from, ConverterType to, string template, string csl)
        {
            if (from != ConverterType.Undefined)
            {
                yield return CreateField("from", from.ToString().ToLower());
                yield return CreateField("ext", from.GetExtension());
            }
            if (to != ConverterType.Undefined)
                yield return CreateField("to", to.ToString().ToLower());
            if (!string.IsNullOrWhiteSpace(template))
                yield return CreateField("template", template);
            if (!string.IsNullOrWhiteSpace(csl))
                yield return CreateField("csl", csl);
        }

        public static IEnumerable<MultipartFormParameter> FromFolder(IDirectoryInfo directoryInfo)
        {
            var item = new WorkspaceItem(directoryInfo, null, null);
            return CreateFormData(item, item, false).Where(e => e != null);
        }

        public static IEnumerable<MultipartFormParameter> FromWebWorkspace(IWorkspace workspace, User user)
        {
            yield return CreateField("token", user.Token);
            //yield return CreateField("workspace", workspace.Settings.Sync);
        }

        public static IEnumerable<MultipartFormParameter> GetWebWorkspaceItem(IWorkspaceItem item, User user)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }
            yield return CreateField("token", user.Token);
            //yield return CreateField("workspace", item.Workspace.Settings.Sync);
            yield return CreateField("name", item.FullName);
        }

        public static IEnumerable<MultipartFormParameter> PostWebWorkspaceItem(IWorkspaceItem item, string file)
        {
            //yield return CreateField("token", item.Workspace.User.Token);
            //yield return CreateField("workspace", item.Workspace.Name);
            yield return CreateField("name", item.FullName);
            yield return CreateFile("content", file);
        }

        public static IEnumerable<MultipartFormParameter> FromWorkspaceItem(IWorkspaceItem workspaceItem, bool onlySelected)
        {
            return CreateFormData(workspaceItem.Parent, workspaceItem, onlySelected).Where(e => e != null).Distinct();
        }

        private static IEnumerable<MultipartFormParameter> CreateFormData(IWorkspaceItem item, IWorkspaceItem root, bool onlySelected)
        {
            if (item is null || item.IsExcluded)
            {
                yield break;
            }
            else if (item.IsDirectory)
            {
                foreach (var child in item.Children)
                {
                    foreach (var data in CreateFormData(child, root, onlySelected))
                    {
                        yield return data;
                    }
                }
            }
            else if (!onlySelected || item.Equals(root))
            {
                yield return CreateFile(item);
            }
        }

        private static string UnixCombine(params string[] values)
        {
            return Path.Combine(values).Replace('\\', '/');
        }
    }

    public class WebDownloadProgress
    {
        public int Length { get; set; }
        public int Current { get; set; }
        public double Progress => Current / (double)Length;

        public WebDownloadProgress(int length, int current)
        {
            Length = length;
            Current = current;
        }
    }
}