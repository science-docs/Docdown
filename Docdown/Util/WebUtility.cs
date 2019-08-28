using Docdown.Model;
using Docdown.Properties;
using Docdown.ViewModel;
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

        public static async Task<HttpResponseMessage> SimpleRequest(string url, HttpMethod method, CancellationToken cancellationToken, params MultipartFormParameter[] postParameters)
        {
            var handler = new WinHttpHandler();
            var client = new HttpClient(handler);

            var request = new HttpRequestMessage
            {
                Method = method,
                RequestUri = new Uri(url)
            };
            request.Headers.UserAgent.Add(UserAgent);

            if (postParameters != null && postParameters.Length > 0)
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

        public static async Task<ConnectionStatus> Ping()
        {
            try
            {
                var res = await GetRequest(Settings.Default.API);
                res.Dispose();
            }
            catch (Exception e)
            {
                ErrorUtility.WriteLog(e);
                return ConnectionStatus.Disconnected;
            }
            return ConnectionStatus.Connected;
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
            return await SimpleRequest(postUrl, HttpMethod.Post, CancellationToken.None, postParameters.ToArray());
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

        public static IEnumerable<MultipartFormParameter> ApiParameter(ConverterType from, ConverterType to, string template, string csl, bool onlySelected)
        {
            if (from != ConverterType.Undefined)
            {
                yield return CreateField("from", from.ToString().ToLower());
                if (!onlySelected)
                {
                    yield return CreateField("ext", from.GetExtension());
                }
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
            return CreateFormData(item, item, null, false).Where(e => e != null);
        }

        // TODO fix this

        public static IEnumerable<MultipartFormParameter> FromWebWorkspace(IWorkspace workspace, User user)
        {
            yield return CreateField("token", user.Token);
            yield return CreateField("workspace", workspace.Settings.Sync);
        }

        public static IEnumerable<MultipartFormParameter> GetWebWorkspaceItem(IWorkspaceItem item, User user)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }
            yield return CreateField("token", user.Token);
            yield return CreateField("workspace", item.Workspace.Settings.Sync);
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
            return CreateFormData(workspaceItem.Parent, workspaceItem, null, onlySelected).Where(e => e != null).Distinct();
        }

        private static IEnumerable<MultipartFormParameter> CreateFormData(IWorkspaceItem item, IWorkspaceItem root, string current, bool onlySelected)
        {
            if (item is null)
            {
                yield break;
            }
            else if (item.IsDirectory)
            {
                string folder = item.Name;
                if (!string.IsNullOrWhiteSpace(current))
                    folder = Path.Combine(current, folder);
                if (item.Equals(root) || item.Equals(root.Parent))
                    folder = null;

                foreach (var child in item.Children)
                {
                    foreach (var data in CreateFormData(child, root, folder, onlySelected))
                    {
                        yield return data;
                    }
                }
            }
            else if (onlySelected && item.Equals(root))
            {
                yield return CreateFile(MainFile, root.FullName);
            }
            else
            {
                string name = item.Name;
                if (!string.IsNullOrWhiteSpace(current))
                    name = Path.Combine(current, name);

                yield return CreateFile(name, item.FullName);
            }
        }
    }
}