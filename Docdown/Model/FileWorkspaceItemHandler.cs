using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Docdown.Util;
using Docdown.Properties;
using System;
using System.IO.Abstractions;
using System.Diagnostics;

namespace Docdown.Model
{
    public class FileWorkspaceItemHandler : IWorkspaceItemHandler
    {
        public int Order => 0;

        public async Task<string> Convert(IWorkspace workspace, CancelToken cancelToken)
        {
            string temp = IOUtility.GetHashFile(workspace.FileSystem.Directory, workspace.Item.FullName);
            var settings = Settings.Default;

            if (settings.UseOfflineCompiler)
            {
                return PandocUtility.Compile(temp, workspace.Item.FullName, settings.LocalTemplate, settings.LocalCsl, "*.md");
            }
            else
            {
                var bytes = await workspace.ConverterService.Convert(MultipartFormParameter
                    .ApiParameter(ConverterType.Markdown, ConverterType.Pdf, settings.Template, settings.Csl)
                    .Concat(MultipartFormParameter
                    .FromWorkspace(workspace)), cancelToken);
                await IOUtility.WriteAllBytes(temp, workspace.FileSystem.File, bytes);
                return temp;
            }
        }

        public Task Delete(IWorkspaceItem item)
        {
            try
            {
                if (item.IsDirectory)
                {
                    item.FileInfo.FileSystem.Directory.Delete(item.FullName, true);
                }
                else
                {
                    item.FileInfo.FileSystem.File.Delete(item.FullName);
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine("Error deleting item: " + e.Message);
            }
            return Task.CompletedTask;
        }

        public byte[] Read(IWorkspaceItem item)
        {
            if (item.IsDirectory)
            {
                throw new InvalidOperationException("Cannot read content of directory");
            }
            return item.FileInfo.FileSystem.File.ReadAllBytes(item.FullName);
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
