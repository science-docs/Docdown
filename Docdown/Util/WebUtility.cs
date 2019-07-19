using Docdown.Model;
using Docdown.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;

namespace Docdown.Util
{
    public static class WebUtility
    {
        private static readonly Encoding encoding = Encoding.UTF8;

        public static string UserAgent { get; } = $"Docdown_v" + typeof(WebUtility).Assembly.GetName().Version.ToString();

        public static string BuildConvertUrl()
        {
            return BuildUrl(Settings.Default.API, "convert");
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

        public static string BuildUrl(params string[] values)
        {
            return Path.Combine(values).Replace('\\', '/');
        }

        public static HttpResponseMessage DeleteRequest(string url, IEnumerable<MultipartFormParameter> postParameters)
        {
            return SimpleRequest(url, HttpMethod.Delete, CancellationToken.None, postParameters.ToArray());
        }

        public static HttpResponseMessage GetRequest(string url, IEnumerable<MultipartFormParameter> postParameters)
        {
            return GetRequest(url, postParameters.ToArray());
        }

        public static HttpResponseMessage SimpleRequest(string url, HttpMethod method, CancellationToken cancellationToken, params MultipartFormParameter[] postParameters)
        {
            var handler = new WinHttpHandler();
            var client = new HttpClient(handler);

            var request = new HttpRequestMessage
            {
                Method = method,
                RequestUri = new Uri(url)
            };
            request.Headers.UserAgent.Add(new ProductInfoHeaderValue("Docdown", "1.0.0"));

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

            var responseTask = client.SendAsync(request, cancellationToken);
            responseTask.Wait();
            var response = responseTask.Result;
            response.EnsureSuccessStatusCode();

            return response;
        }

        public static HttpResponseMessage GetRequest(string url, params MultipartFormParameter[] postParameters)
        {
            return SimpleRequest(url, HttpMethod.Get, CancellationToken.None, postParameters);
        }

        public static string SimpleTextRequest(string url, params MultipartFormParameter[] postParameters)
        {
            using (var res = GetRequest(url, postParameters))
            {
                var task = res.Content.ReadAsStringAsync();
                task.Wait();
                return task.Result;
            }
        }

        public static ConnectionStatus Ping()
        {
            try
            {
                GetRequest(Settings.Default.API).Dispose();
            }
            catch
            {
                return ConnectionStatus.Disconnected;
            }
            return ConnectionStatus.Connected;
        }

        public static HttpResponseMessage PostRequest(string postUrl, IEnumerable<MultipartFormParameter> postParameters)
        {
            return SimpleRequest(postUrl, HttpMethod.Post, CancellationToken.None, postParameters.ToArray());
        }

        public static HttpResponseMessage PostRequest(string postUrl, CancellationToken cancellationToken, IEnumerable<MultipartFormParameter> postParameters)
        {
            return SimpleRequest(postUrl, HttpMethod.Post, cancellationToken, postParameters.ToArray());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="postUrl"></param>
        /// <param name="userAgent"></param>
        /// <param name="postParameters"></param>
        /// <returns></returns>
        /// <exception cref="WebException"/>
        public static HttpWebRequest MultipartFormDataPost(string postUrl, params MultipartFormParameter[] postParameters)
        {
            string formDataBoundary = $"----------{Guid.NewGuid()}";
            string contentType = "multipart/form-data; boundary=" + formDataBoundary;

            byte[] formData = GetMultipartFormData(postParameters, formDataBoundary);
            return PostForm(postUrl, UserAgent, contentType, formData);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="postUrl"></param>
        /// <param name="userAgent"></param>
        /// <param name="contentType"></param>
        /// <param name="formData"></param>
        /// <returns></returns>
        /// <exception cref="WebException"/>
        private static HttpWebRequest PostForm(string postUrl, string userAgent, string contentType, byte[] formData)
        {
            var request = WebRequest.CreateHttp(postUrl);

            // Set up the request properties.
            request.Method = "POST";
            request.ContentType = contentType;
            request.UserAgent = userAgent;
            request.CookieContainer = new CookieContainer();
            request.ContentLength = formData.Length;
            request.Timeout = 120_000;

            // Send the form data to the request.
            using (var requestStream = request.GetRequestStream())
            {
                requestStream.Write(formData, 0, formData.Length);
            }

            return request;
        }

        private static byte[] GetMultipartFormData(IEnumerable<MultipartFormParameter> postParameters, string boundary)
        {
            bool needsCLRF = false;
            string CRLF = Environment.NewLine;

            using (var formDataStream = new MemoryStream())
            {
                foreach (var param in postParameters)
                {
                    if (needsCLRF)
                        Write(formDataStream, CRLF);

                    needsCLRF = true;

                    if (param.Type == MultipartFormParameterType.File)
                    {
                        // Add just the first part of this param, since we will write the file data directly to the Stream
                        string header = $"--{boundary}{CRLF}Content-Disposition: form-data; name=\"{param.Name}\"; filename=\"{param.FileName}\"{CRLF}Content-Type: {param.ContentType}{CRLF}{CRLF}";

                        Write(formDataStream, header);

                        // Write the file data directly to the Stream, rather than serializing it to a string.
                        Write(formDataStream, param.Data);
                    }
                    else if (param.Type == MultipartFormParameterType.Field)
                    {
                        if (param.Array)
                        {
                            foreach (var value in param.Value)
                            {
                                AddField(formDataStream, param.Name + "[]", value);
                            }
                        }
                        else
                        {
                            AddField(formDataStream, param.Name, param.Value[0]);
                        }
                    }
                }

                // Add the end of the request.  Start with a newline
                Write(formDataStream, $"{CRLF}--{boundary}--{CRLF}");

                return formDataStream.ToArray();
            }

            void AddField(MemoryStream ms, string name, string value)
            {
                string postData = $"--{boundary}{CRLF}Content-Disposition: form-data; name=\"{name}\"{CRLF}{CRLF}{value}";
                Write(ms, postData);
            }
        }

        private static void Write(Stream stream, string text)
        {
            Write(stream, encoding.GetBytes(text));
        }

        private static void Write(Stream stream, byte[] data)
        {
            stream.Write(data, 0, data.Length);
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

        public static IEnumerable<MultipartFormParameter> FromFolder(string folderPath)
        {
            var item = new FileWorkspaceItem(folderPath);
            return CreateFormData(item, item, null, false).Where(e => e != null);
        }

        public static IEnumerable<MultipartFormParameter> FromWebWorkspace(WebWorkspace workspace)
        {
            yield return CreateField("token", workspace.User.Token);
            yield return CreateField("workspace", workspace.Name);
        }

        public static IEnumerable<MultipartFormParameter> GetWebWorkspaceItem(WebWorkspaceItem item)
        {
            yield return CreateField("token", item.Workspace.User.Token);
            yield return CreateField("workspace", item.Workspace.Name);
            yield return CreateField("name", item.FullName);
        }

        public static IEnumerable<MultipartFormParameter> PostWebWorkspaceItem(WebWorkspaceItem item, string file)
        {
            yield return CreateField("token", item.Workspace.User.Token);
            yield return CreateField("workspace", item.Workspace.Name);
            yield return CreateField("name", item.FullName);
            yield return CreateFile("content", file);
        }

        public static IEnumerable<MultipartFormParameter> FromWorkspaceItem(FileWorkspaceItem workspaceItem, bool onlySelected)
        {
            return CreateFormData(workspaceItem.Parent, workspaceItem, null, onlySelected).Where(e => e != null).Distinct();
        }

        private static IEnumerable<MultipartFormParameter> CreateFormData(FileWorkspaceItem item, FileWorkspaceItem root, string current, bool onlySelected)
        {
            if (item is null)
            {
                yield break;
            }
            else if (item.IsDirectory)
            {
                string folder = item.FileSystemInfo.Name;
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
                yield return CreateFile(MainFile, root.FileSystemInfo.FullName);
            }
            else
            {
                var fsi = item.FileSystemInfo;
                string name = fsi.Name;

                if (!string.IsNullOrWhiteSpace(current))
                    name = Path.Combine(current, name);

                yield return CreateFile(name, fsi.FullName);
            }
        }
    }
}