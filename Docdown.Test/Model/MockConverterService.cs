﻿using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Docdown.Util;

namespace Docdown.Model.Test
{
    public class MockConverterService : IConverterService
    {
        public string Url { get; set; }

        public Task<ConnectionStatus> Connect()
        {
            return Task.FromResult(ConnectionStatus.Connected);
        }

        public async Task<byte[]> Convert(IEnumerable<MultipartFormParameter> parameters, CancelToken cancelToken)
        {
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Docdown.Test.Files.Result.txt");
            using (var ms = new MemoryStream())
            {
                await stream.CopyToAsync(ms);
                return ms.ToArray();
            }
        }

        public Task<string[]> LoadCsls()
        {
            return Task.FromResult(new string[] { "IEEE" });
        }

        public async Task<Template[]> LoadTemplates()
        {
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Docdown.Test.Files.Result.txt");
            using (var sr = new StreamReader(stream))
            {
                var text = await sr.ReadToEndAsync();
                return Template.FromJson(text);
            }
        }
    }
}