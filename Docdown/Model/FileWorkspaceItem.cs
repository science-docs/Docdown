using Docdown.Properties;
using Docdown.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Docdown.Model
{
    public class FileWorkspaceItem : WorkspaceItem<FileWorkspaceItem>
    {
        public FileSystemInfo FileSystemInfo { get; set; }
        public override string Name => FileSystemInfo.Name;
        public override string FullName => FileSystemInfo.FullName;
        public bool IsHidden => (FileSystemInfo.Attributes & FileAttributes.Hidden) > 0 || (Parent != null && Parent.IsHidden);

        public FileWorkspaceItem(string folderPath)
            : this(new DirectoryInfo(folderPath), true)
        {

        }

        public FileWorkspaceItem(FileSystemInfo fileSystemInfo)
            : this(fileSystemInfo, false)
        {

        }

        public FileWorkspaceItem(FileSystemInfo fileSystemInfo, bool recursively)
        {
            FileSystemInfo = fileSystemInfo ??
                throw new ArgumentNullException(nameof(fileSystemInfo));

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
                switch (fileInfo.Extension)
                {
                    case ".md":
                    case ".markdown":
                        Type = WorkspaceItemType.Markdown;
                        break;
                    case ".tex":
                    case ".latex":
                        Type = WorkspaceItemType.Latex;
                        break;
                    case ".pdf":
                        Type = WorkspaceItemType.Pdf;
                        break;
                    case ".docx":
                        Type = WorkspaceItemType.Docx;
                        break;
                    case ".txt":
                    case ".ini":
                    case ".bat":
                    case ".sh":
                    case ".json":
                    case ".xml":
                    case ".xaml":
                        Type = WorkspaceItemType.Text;
                        break;
                    case ".png":
                    case ".jpg":
                    case ".jpeg":
                    case ".gif":
                    case ".webp":
                        Type = WorkspaceItemType.Image;
                        break;
                    case ".mp4":
                    case ".webm":
                    case ".mkv":
                        Type = WorkspaceItemType.Video;
                        break;
                    case ".mp3":
                    case ".flac":
                    case ".ogg":
                        Type = WorkspaceItemType.Audio;
                        break;
                    case ".bib":
                    case ".bibtex":
                        Type = WorkspaceItemType.Bibliography;
                        break;
                    default:
                        Type = WorkspaceItemType.Other;
                        break;
                }
            }
        }

        public override string Convert(CancelToken cancelToken)
        {
            var folder = Path.GetDirectoryName(FileSystemInfo.FullName);

            string temp = IOUtility.GetHashFile(FullName);
            var settings = Settings.Default;
            var onlySelected = settings.CompileOnlySelected;

            if (settings.UseOfflineCompiler)
            {
                return PandocUtility.Compile(temp, TopParent.FileSystemInfo.FullName, settings.LocalTemplate, settings.LocalCsl, "*.md");
            }
            else
            {
                var req = WebUtility.MultipartFormDataPost(WebUtility.BuildConvertUrl(),
                MultipartFormParameter.ApiParameter(FromType, ToType, settings.Template, settings.Csl, onlySelected).Concat(
                MultipartFormParameter.FromWorkspaceItem(this, onlySelected)));

                if (cancelToken != null)
                {
                    cancelToken.Canceled += AbortRequest;
                }

                using (var res = req.GetResponse())
                using (var rs = res.GetResponseStream())
                {
                    using (var fs = File.Open(temp, FileMode.Create))
                    {
                        rs.CopyTo(fs);
                    }
                }

                return temp;

                void AbortRequest(object sender, EventArgs e)
                {
                    req.Abort();
                }
            }
        }

        public override void Delete()
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
        }

        public override byte[] Read()
        {
            return File.ReadAllBytes(FileSystemInfo.FullName);
        }

        public override void Save(string text)
        {
            File.WriteAllText(FileSystemInfo.FullName, text);
        }

        public override void Rename(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName) ||
                !IOUtility.IsValidFileName(newName))
                throw new ArgumentException("Invalid name");
            if (newName == FileSystemInfo.Name)
                return;

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
        }

        public override void Update()
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
        }

        public override FileWorkspaceItem CreateNewFile(string name, string autoExtension = null, byte[] content = null)
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
            File.WriteAllBytes(fullName, content);

            var fileInfo = new FileInfo(fullName);
            var item = new FileWorkspaceItem(fileInfo)
            {
                Parent = this
            };
            Children.Add(item);
            return item;
        }

        public override FileWorkspaceItem CreateNewDirectory(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("message", nameof(name));
            if (Type != WorkspaceItemType.Directory)
                throw new InvalidOperationException("Only Directories can contain other directories");

            string fullName = Path.Combine(FileSystemInfo.FullName, name);

            Directory.CreateDirectory(fullName);
            var fileInfo = new DirectoryInfo(fullName);
            var item = new FileWorkspaceItem(fileInfo)
            {
                Parent = this
            };
            Children.Add(item);
            return item;
        }

        public override FileWorkspaceItem CopyExistingItem(string path)
        {
            var fileName = Path.GetFileName(path);
            var fullNewName = Path.Combine(FileSystemInfo.FullName, fileName);

            if (Children.Any(e => e.Children.First().FileSystemInfo.Name == fileName))
            {
                File.Replace(path, fullNewName, null);
            }
            else
            {
                File.Copy(path, fullNewName);
            }

            var fileInfo = new FileInfo(fullNewName);
            var item = new FileWorkspaceItem(fileInfo)
            {
                Parent = this
            };
            Children.Add(item);
            return item;
        }

        public override FileWorkspaceItem CopyExistingFolder(string path)
        {
            var item = CreateNewDirectory(Path.GetFileName(path));
            foreach (var file in Directory.GetFiles(path))
            {
                var child = CopyExistingItem(file);
                item.Children.Add(child);
                child.Parent = item;
            }
            foreach (var directory in Directory.GetDirectories(path))
            {
                var child = CopyExistingFolder(directory);
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
            foreach (var dir in directoryInfo.EnumerateFileSystemInfos())
            {
                if (dir.Name != ".git")
                {
                    yield return new FileWorkspaceItem(dir, true) { Parent = parent };
                }
            }
        }
    }
}
