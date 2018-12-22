using Docdown.Model;
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

        public static HttpWebResponse MultipartFormDataPost(string postUrl, string userAgent, params MultipartFormParameter[] postParameters)
        {
            string formDataBoundary = $"----------{Guid.NewGuid()}";
            string contentType = "multipart/form-data; boundary=" + formDataBoundary;

            byte[] formData = GetMultipartFormData(postParameters, formDataBoundary);
            string text = Encoding.UTF8.GetString(formData);
            return PostForm(postUrl, userAgent, contentType, formData);
        }

        private static HttpWebResponse PostForm(string postUrl, string userAgent, string contentType, byte[] formData)
        {
            var request = WebRequest.Create(postUrl) as HttpWebRequest 
                ?? throw new NullReferenceException("request is not a http request");

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

            return request.GetResponse() as HttpWebResponse;
        }

        private static byte[] GetMultipartFormData(IEnumerable<MultipartFormParameter> postParameters, string boundary)
        {
            bool needsCLRF = false;
            const string CLRF = "\r\n";

            using (var formDataStream = new MemoryStream())
            {
                foreach (var param in postParameters)
                {
                    // Thanks to feedback from commenters, add a CRLF to allow multiple parameters to be added.
                    // Skip it on the first parameter, add it to subsequent parameters.
                    if (needsCLRF)
                        Write(formDataStream, CLRF);

                    needsCLRF = true;

                    if (param.Type == MultipartFormParameterType.File)
                    {
                        // Add just the first part of this param, since we will write the file data directly to the Stream
                        string header = $"--{boundary}{CLRF}Content-Disposition: form-data; name=\"{param.Name}\"; filename=\"{param.FileName}\"{CLRF}Content-Type: {param.ContentType}{CLRF}{CLRF}";

                        Write(formDataStream, header);

                        // Write the file data directly to the Stream, rather than serializing it to a string.
                        Write(formDataStream, param.Data);
                    }
                    else if (param.Type == MultipartFormParameterType.Field)
                    {
                        string postData = $"--{boundary}{CLRF}Content-Disposition: form-data; name=\"{param.Name}\"{CLRF}{CLRF}{param.Value}";
                        Write(formDataStream, postData);
                    }
                }

                // Add the end of the request.  Start with a newline
                Write(formDataStream, $"{CLRF}--{boundary}--{CLRF}");

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

        private const string cType = "application/octet-stream";
        private const string MainFile = "content";

        private MultipartFormParameter() { }

        public static MultipartFormParameter CreateField(string name, string value)
        {
            return new MultipartFormParameter
            {
                Name = name,
                Value = value,
                Type = MultipartFormParameterType.Field
            };
        }

        public static MultipartFormParameter CreateFile(string name, string fileName)
        {
            return CreateFile(name, fileName, cType);
        }

        public static MultipartFormParameter CreateFile(string name, string fileName, string contentType)
        {
            return CreateFile(name, fileName, File.ReadAllBytes(fileName), contentType);
        }

        public static MultipartFormParameter CreateFile(string name, string fileName, byte[] file)
        {
            return CreateFile(name, fileName, file, cType);
        }

        public static MultipartFormParameter CreateFile(string name, string fileName, byte[] file, string contentType)
        {
            return new MultipartFormParameter
            {
                Name = name,
                Data = file,
                FileName = fileName,
                ContentType = contentType,
                Type = MultipartFormParameterType.File
            };
        }

        public static IEnumerable<MultipartFormParameter> FromWorkspaceItem(WorkspaceItem workspaceItem)
        {
            return CreateFormData(workspaceItem.Parent, workspaceItem, null);
        }

        private static IEnumerable<MultipartFormParameter> CreateFormData(WorkspaceItem item, WorkspaceItem root, string current)
        {
            if (item == null)
            {
                yield break;
            }
            if (item == root)
            {
                yield return CreateFile(MainFile, item.FileSystemInfo.FullName);
            }
            else if (item.Type == WorkspaceItemType.Directory)
            {
                string folder = item.FileSystemInfo.Name;
                if (!string.IsNullOrWhiteSpace(current))
                    folder = Path.Combine(current, folder);

                foreach (var child in item.Children)
                {
                    foreach (var data in CreateFormData(child, root, folder))
                    {
                        yield return data;
                    }
                }
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