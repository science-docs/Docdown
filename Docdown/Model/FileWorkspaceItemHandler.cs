using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Docdown.Util;
using Docdown.Properties;
using System;
using System.IO.Abstractions;

namespace Docdown.Model
{
    public class FileWorkspaceItemHandler : IWorkspaceItemHandler
    {
        public int Order => 0;

        public async Task<string> Convert(IWorkspaceItem item, CancelToken cancelToken)
        {
            if (item.IsDirectory)
            {
                throw new InvalidOperationException("Can only convert files");
            }

            string temp = IOUtility.GetHashFile(item.Workspace.FileSystem.Directory, item.FullName);
            var settings = Settings.Default;
            var onlySelected = settings.CompileOnlySelected;

            if (settings.UseOfflineCompiler)
            {
                return PandocUtility.Compile(temp, item.TopParent.FullName, settings.LocalTemplate, settings.LocalCsl, "*.md");
            }
            else
            {
                var bytes = await item.Workspace.ConverterService.Convert(MultipartFormParameter
                    .ApiParameter(item.FromType, item.ToType, settings.Template, settings.Csl, onlySelected)
                    .Concat(MultipartFormParameter
                    .FromWorkspaceItem(item, onlySelected)), cancelToken);
                await IOUtility.WriteAllBytes(temp, item.FileInfo.FileSystem.File, bytes);
                return temp;
            }
        }

        public Task Delete(IWorkspaceItem item)
        {
            if (item.IsDirectory)
            {
                item.FileInfo.FileSystem.Directory.Delete(item.FullName);
            }
            else
            {
                item.FileInfo.FileSystem.File.Delete(item.FullName);
            }
            return Task.CompletedTask;
        }

        public async Task<byte[]> Read(IWorkspaceItem item)
        {
            if (item.IsDirectory)
            {
                throw new InvalidOperationException("Cannot read content of directory");
            }

            return await IOUtility.ReadAllBytes(item.FileInfo as IFileInfo);
        }

        public Task Rename(IWorkspaceItem item, string newName)
        {
            var dir = Path.GetDirectoryName(item.FullName);
            var name = Path.Combine(dir, newName);
            var fs = item.FileInfo.FileSystem;

            if (item.IsDirectory)
            {
                item.FileInfo.FileSystem.Directory.Move(item.FullName, name);
                item.FileInfo = fs.DirectoryInfo.FromDirectoryName(name);
            }
            else
            {
                item.FileInfo.FileSystem.File.Move(item.FullName, name);
                item.FileInfo = fs.FileInfo.FromFileName(name);
            }

            return Task.CompletedTask;
        }

        public async Task Save(IWorkspaceItem item, byte[] bytes)
        {
            if (item.IsDirectory)
            {
                throw new InvalidOperationException("Cannot save the contents of a directory");
            }

            await IOUtility.WriteAllBytes(item.FileInfo as IFileInfo, bytes);
        }
    }
}
