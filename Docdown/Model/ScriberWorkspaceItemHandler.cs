using System;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Threading.Tasks;
using Docdown.Net;
using Docdown.Util;
using Scriber.Language;
using Scriber;
using Scriber.Engine;

namespace Docdown.Model
{
    public class ScriberWorkspaceItemHandler : IWorkspaceItemHandler
    {
        private static readonly Context context = new Context();

        static ScriberWorkspaceItemHandler()
        {
            var loader = new ReflectionLoader();
            loader.Discover(context, typeof(Context).Assembly);
        }

        public int Order => 0;

        public async Task<string> Convert(IWorkspace workspace, CancelToken cancelToken)
        {
            try
            {
                string temp = IOUtility.GetHashFile(workspace.FileSystem.Directory, workspace.Item.FullName);
                var selectedFile = workspace.SelectedItem.FileInfo;
                var fullName = selectedFile.FullName;

                var text = workspace.FileSystem.File.ReadAllText(fullName);
                var elements = Parser.ParseFromString(text);
                var result = Compiler.Compile(context, elements.Elements);
                var document = result.Document;
                document.Run();
                var pdf = document.ToPdf();
                await IOUtility.WriteAllBytes(temp, workspace.FileSystem.File, pdf);
                return temp;
            }
            catch
            {
                throw;
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
