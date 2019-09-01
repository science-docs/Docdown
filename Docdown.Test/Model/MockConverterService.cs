using System;
using System.Collections.Generic;
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

        public Task<byte[]> Convert(IEnumerable<MultipartFormParameter> parameters, CancelToken cancelToken)
        {
            return Task.FromResult(new byte[0]);
        }

        public Task<string[]> LoadCsls()
        {
            throw new NotImplementedException();
        }

        public Task<Template[]> LoadTemplates()
        {
            throw new NotImplementedException();
        }
    }
}
