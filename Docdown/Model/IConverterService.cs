using Docdown.Net;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Docdown.Model
{
    public interface IConverterService
    {
        string Url { get; set; }
        Task<ConnectionStatus> Connect();
        Task<byte[]> Convert(IEnumerable<MultipartFormParameter> parameters, CancelToken cancelToken);
        Task<Template[]> LoadTemplates();
        Task<string[]> LoadCsls();
    }
}
