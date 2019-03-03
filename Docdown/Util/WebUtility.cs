using Docdown.Model;
using Docdown.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

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

        public static HttpWebResponse SimpleGetRequest(string url)
        {
            var request = WebRequest.CreateHttp(url);

            request.Method = "GET";
            request.UserAgent = UserAgent;
            request.CookieContainer = new CookieContainer();
            request.Timeout = 60_000;

            return request.GetResponse() as HttpWebResponse;
        }

        public static ConnectionStatus Ping()
        {
            try
            {
                SimpleGetRequest(Settings.Default.API).Dispose();
            }
            catch
            {
                return ConnectionStatus.Disconnected;
            }
            return ConnectionStatus.Connected;
        }

        public static HttpWebRequest MultipartFormDataPost(string postUrl, IEnumerable<MultipartFormParameter> postParameters)
        {
            return MultipartFormDataPost(postUrl, postParameters.ToArray());
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
            string text = Encoding.UTF8.GetString(formData);
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
            request.Timeout = 60_000;

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
                        string postData = $"--{boundary}{CRLF}Content-Disposition: form-data; name=\"{param.Name}\"{CRLF}{CRLF}{param.Value}";
                        Write(formDataStream, postData);
                    }
                }

                // Add the end of the request.  Start with a newline
                Write(formDataStream, $"{CRLF}--{boundary}--{CRLF}");

                return formDataStream.ToArray();
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
        public string Value { get; set; }
        public MultipartFormParameterType Type { get; set; }

        private const string BinaryType = "application/octet-stream";
        private const string MainFile = "content";

        private MultipartFormParameter() { }

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
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return new MultipartFormParameter
            {
                Name = name,
                Value = value,
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
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (fileName == null)
                throw new ArgumentNullException(nameof(fileName));
            if (file == null)
                throw new ArgumentNullException(nameof(file));
            if (contentType == null)
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

        public static IEnumerable<MultipartFormParameter> FromFolder(string folderPath)
        {
            var item = new WorkspaceItem(folderPath);
            return CreateFormData(item, item, null, false).Where(e => e != null);
        }

        public static IEnumerable<MultipartFormParameter> FromWorkspaceItem(WorkspaceItem workspaceItem, bool onlySelected)
        {
            return CreateFormData(workspaceItem, workspaceItem.Parent, null, onlySelected).Where(e => e != null).Distinct();
        }

        private static IEnumerable<MultipartFormParameter> CreateFormData(WorkspaceItem item, WorkspaceItem parent, string current, bool onlySelected)
        {
            if (parent == null)
            {
                yield break;
            }
            else if (parent.IsDirectory())
            {
                string folder = parent.FileSystemInfo.Name;
                if (!string.IsNullOrWhiteSpace(current))
                    folder = Path.Combine(current, folder);
                if (parent == item || parent == item.Parent)
                    folder = null;

                foreach (var child in parent.Children)
                {
                    foreach (var data in CreateFormData(child, item, folder, onlySelected))
                    {
                        yield return data;
                    }
                }
            }
            else if (onlySelected && item == parent)
            {
                yield return CreateFile(MainFile, item.FileSystemInfo.FullName);
            }
            else
            {
                var fsi = parent.FileSystemInfo;
                string name = fsi.Name;

                if (!string.IsNullOrWhiteSpace(current))
                    name = Path.Combine(current, name);

                yield return CreateFile(name, fsi.FullName);
            }
        }
    }
}