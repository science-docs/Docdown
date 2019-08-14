using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Docdown.Util;
using Docdown.Properties;
using System.Threading;

namespace Docdown.Model
{
    public class FileWorkspaceItemHandler : IWorkspaceItemHandler
    {
        public int Order => 0;


        public async Task<string> Convert(IWorkspaceItem item, CancelToken cancelToken)
        {
            string temp = IOUtility.GetHashFile(item.FullName);
            var settings = Settings.Default;
            var onlySelected = settings.CompileOnlySelected;

            if (settings.UseOfflineCompiler)
            {
                return PandocUtility.Compile(temp, item.TopParent.FullName, settings.LocalTemplate, settings.LocalCsl, "*.md");
            }
            else
            {
                var token = cancelToken?.ToCancellationToken() ?? CancellationToken.None;
                using (var req = await WebUtility.PostRequest(WebUtility.BuildConvertUrl(), token,
                    MultipartFormParameter.ApiParameter(item.FromType, item.ToType, settings.Template, settings.Csl, onlySelected).Concat(
                    MultipartFormParameter.FromWorkspaceItem(item, onlySelected))))
                {
                    using (var fs = File.Open(temp, FileMode.Create))
                    {
                        await req.Content.CopyToAsync(fs);
                    }
                }
                return temp;
            }
        }

        public Task Delete(IWorkspaceItem item)
        {
            if (item.IsDirectory)
            {
                Directory.Delete(item.FullName);
            }
            else
            {
                File.Delete(item.FullName);
            }
            return Task.CompletedTask;
        }

        public async Task<byte[]> Read(IWorkspaceItem item)
        {
            return await IOUtility.ReadAllBytes(item.FullName);
        }

        public Task Rename(IWorkspaceItem item, string newName)
        {
            var dir = Path.GetDirectoryName(item.FullName);
            var name = Path.Combine(dir, newName);

            if (item.IsDirectory)
            {
                Directory.Move(item.FullName, name);
                item.FileInfo = new DirectoryInfo(name);
            }
            else
            {
                File.Move(item.FullName, name);
                item.FileInfo = new FileInfo(name);
            }

            return Task.CompletedTask;
        }

        public async Task Save(IWorkspaceItem item, byte[] bytes)
        {
            await IOUtility.WriteAllBytes(item.FullName, bytes);
        }
    }
}
