using Docdown.Properties;
using Docdown.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Docdown.Model
{
    public class FileWorkspaceItem : WorkspaceItem<FileWorkspaceItem>
    {
        public FileSystemInfo FileSystemInfo { get; set; }
        public override string Name => FileSystemInfo.Name;
        public override string FullName => FileSystemInfo.FullName;
        public bool IsHidden => (FileSystemInfo.Attributes & FileAttributes.Hidden) > 0 || (Parent != null && Parent.IsHidden);

        public FileWorkspaceItem(string folderPath)
            : this(new DirectoryInfo(folderPath), null, true)
        {

        }

        public FileWorkspaceItem(FileSystemInfo fileSystemInfo, FileWorkspaceItem parent)
            : this(fileSystemInfo, parent, false)
        {

        }

        private FileWorkspaceItem(FileSystemInfo fileSystemInfo, FileWorkspaceItem parent, bool recursively)
        {
            FileSystemInfo = fileSystemInfo ??
                throw new ArgumentNullException(nameof(fileSystemInfo));
            Parent = parent;

            if (fileSystemInfo is DirectoryInfo directoryInfo)
            {
                Type = WorkspaceItemType.Directory;

                if (recursively)
                {
                    Children.AddRange(FromDirectory(directoryInfo, this));
                }
            }
            else if (fileSystemInfo is FileInfo fileInfo)
            {
                SetType(fileInfo.Extension);
            }
        }

        public async override Task<string> Convert(CancelToken cancelToken)
        {
            var folder = Path.GetDirectoryName(FullName);

            string temp = IOUtility.GetHashFile(FullName);
            var settings = Settings.Default;
            var onlySelected = settings.CompileOnlySelected;

            if (settings.UseOfflineCompiler)
            {
                return PandocUtility.Compile(temp, TopParent.FullName, settings.LocalTemplate, settings.LocalCsl, "*.md");
            }
            else
            {
                var token = cancelToken?.ToCancellationToken() ?? CancellationToken.None;
                var req = WebUtility.PostRequest(WebUtility.BuildConvertUrl(), token,
                MultipartFormParameter.ApiParameter(FromType, ToType, settings.Template, settings.Csl, onlySelected).Concat(
                MultipartFormParameter.FromWorkspaceItem(this, onlySelected)));

                using (var fs = File.Open(temp, FileMode.Create))
                {
                    await req.Content.CopyToAsync(fs);
                }

                return temp;
            }
        }

        public override Task Delete()
        {
            string fullName = FileSystemInfo.FullName;

            if (IsDirectory)
            {
                Directory.Delete(fullName, true);
            }
            else
            {
                File.Delete(fullName);
            }

            Parent.Children.Remove(this);
            Parent = null;
            return null;
        }

        public async override Task<byte[]> Read()
        {
            return await IOUtility.ReadAllBytes(FullName);
        }

        public async override Task Save(string text)
        {
            await IOUtility.WriteAllText(FullName, text);
        }

        public override Task Rename(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName) ||
                !IOUtility.IsValidFileName(newName))
                throw new ArgumentException("Invalid name");
            if (newName == FileSystemInfo.Name)
                return null;

            string oldName = FileSystemInfo.FullName;
            string parentName = Path.GetDirectoryName(oldName);
            string fullNewName = Path.Combine(parentName, newName);

            if (IsDirectory)
            {
                Directory.Move(oldName, fullNewName);
                FileSystemInfo = new DirectoryInfo(fullNewName);
                Children.ForEach(e => e.Update());
            }
            else
            {
                File.Move(oldName, fullNewName);
                FileSystemInfo = new FileInfo(fullNewName);
            }
            return null;
        }

        public override Task Update()
        {
            var name = FileSystemInfo.Name;
            var parentDirectory = Parent.FileSystemInfo.FullName;
            var newName = Path.Combine(parentDirectory, name);
            if (Directory.Exists(newName))
            {
                FileSystemInfo = new DirectoryInfo(newName);
                Children.ForEach(e => e.Update());
            }
            else
            {
                FileSystemInfo = new FileInfo(newName);
            }
            return null;
        }

        public async override Task<FileWorkspaceItem> CreateNewFile(string name, string autoExtension = null, byte[] content = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("message", nameof(name));
            if (Type != WorkspaceItemType.Directory)
                throw new InvalidOperationException("Only Directories can contain files");

            if (autoExtension != null && !name.EndsWith(autoExtension))
            {
                name += autoExtension;
            }

            string fullName = Path.Combine(FileSystemInfo.FullName, name);

            if (content is null)
            {
                content = new byte[0];
            }

            await IOUtility.WriteAllBytes(fullName, content);

            if (FindChild(fullName, out var existing))
            {
                return existing;
            }

            var fileInfo = new FileInfo(fullName);
            var item = new FileWorkspaceItem(fileInfo, this);
            Children.Add(item);
            return item;
        }

        public override Task<FileWorkspaceItem> CreateNewDirectory(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("message", nameof(name));
            if (Type != WorkspaceItemType.Directory)
                throw new InvalidOperationException("Only Directories can contain other directories");

            string fullName = Path.Combine(FileSystemInfo.FullName, name);

            if (FindChild(fullName, out var existing))
            {
                return Task.FromResult(existing);
            }

            Directory.CreateDirectory(fullName);
            var fileInfo = new DirectoryInfo(fullName);
            var item = new FileWorkspaceItem(fileInfo, this);
            Children.Add(item);
            return Task.FromResult(item);
        }

        private bool FindChild(string fullName, out FileWorkspaceItem item)
        {
            item = Children.FirstOrDefault(e => e.FullName == fullName);
            return item != null;
        }

        public override Task<FileWorkspaceItem> CopyExistingItem(string path)
        {
            var fileName = Path.GetFileName(path);
            var fullNewName = Path.Combine(FileSystemInfo.FullName, fileName);

            if (Children.Any(e => e.Name == fileName))
            {
                File.Replace(path, fullNewName, null);
            }
            else
            {
                File.Copy(path, fullNewName);
            }

            var fileInfo = new FileInfo(fullNewName);
            var item = new FileWorkspaceItem(fileInfo, this);
            Children.Add(item);
            return Task.FromResult(item);
        }

        public async override Task<FileWorkspaceItem> CopyExistingFolder(string path)
        {
            var item = await CreateNewDirectory(Path.GetFileName(path));
            foreach (var file in Directory.GetFiles(path))
            {
                var child = await CopyExistingItem(file);
                item.Children.Add(child);
                child.Parent = item;
            }
            foreach (var directory in Directory.GetDirectories(path))
            {
                var child = await CopyExistingFolder(directory);
                item.Children.Add(child);
                child.Parent = item;
            }
            return item;
        }

        public override string ToString()
        {
            return FileSystemInfo?.FullName ?? base.ToString();
        }

        private ConverterType FromFileType()
        {
            switch (Type)
            {
                case WorkspaceItemType.Markdown:
                    return ConverterType.Markdown;
                case WorkspaceItemType.Latex:
                    return ConverterType.Latex;
                default:
                    return ConverterType.Text;
            }
        }

        private static IEnumerable<FileWorkspaceItem> FromDirectory(DirectoryInfo directoryInfo, FileWorkspaceItem parent)
        {
            foreach (var dir in directoryInfo.EnumerateFileSystemInfos().Where(e => !e.Name.StartsWith(".")))
            {
                yield return new FileWorkspaceItem(dir, parent, true);
            }
        }
    }
}