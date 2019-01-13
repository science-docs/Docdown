using Docdown.Properties;
using Docdown.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Docdown.Model
{
    public enum WorkspaceItemType
    {
        Other,
        Bibliography,
        Markdown,
        Latex,
        Pdf,
        Directory,
        Image,
        Video,
        Audio,
        Text,
        Docx
    }

    public class WorkspaceItem
    {
        public FileSystemInfo FileSystemInfo { get; set; }
        public WorkspaceItemType Type { get; set; }
        public List<WorkspaceItem> Children { get; } = new List<WorkspaceItem>();
        public WorkspaceItem Parent { get; private set; }
        public ConverterType FromType => FromFileType();
        public ConverterType ToType { get; set; } = ConverterType.Pdf;

        public WorkspaceItem()
        {

        }

        public WorkspaceItem(string folderPath)
            : this(new DirectoryInfo(folderPath), true)
        {

        }

        public WorkspaceItem(FileSystemInfo fileSystemInfo)
            : this(fileSystemInfo, false)
        {

        }

        public WorkspaceItem(FileSystemInfo fileSystemInfo, bool recursively)
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

        public string Convert()
        {
            var folder = Path.GetDirectoryName(FileSystemInfo.FullName);
            
            string temp = IOUtility.GetTempFile();
            using (var res = WebUtility.MultipartFormDataPost(WebUtility.BuildConvertUrl(),
                MultipartFormParameter.ApiParameter(FromType, ToType, Settings.Default.Template).Concat(
                MultipartFormParameter.FromWorkspaceItem(this))))
            using (var rs = res.GetResponseStream())
            {
                using (var fs = File.OpenWrite(temp))
                {
                    rs.CopyTo(fs);
                }
            }

            return temp;
        }

        public void Rename(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
                throw new ArgumentException("Invalid name");

            string oldName = FileSystemInfo.FullName;
            string parentName = Path.GetDirectoryName(oldName);
            string fullNewName = Path.Combine(parentName, newName);

            if (IsDirectory())
            {
                Directory.Move(oldName, fullNewName);
            }
            else
            {
                File.Move(oldName, fullNewName);
            }
        }

        public WorkspaceItem CreateNewFile(string name, string autoExtension = null, string content = null)
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

            if (content == null)
            {
                content = string.Empty;
            }
            File.WriteAllText(fullName, content);

            var fileInfo = new FileInfo(fullName);
            var item = new WorkspaceItem(fileInfo)
            {
                Parent = this
            };
            Children.Add(item);
            return item;
        }

        public WorkspaceItem CreateNewDirectory(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("message", nameof(name));
            if (Type != WorkspaceItemType.Directory)
                throw new InvalidOperationException("Only Directories can contain other directories");

            string fullName = Path.Combine(FileSystemInfo.FullName, name);

            Directory.CreateDirectory(fullName);
            var fileInfo = new DirectoryInfo(fullName);
            var item = new WorkspaceItem(fileInfo);
            Children.Add(item);
            return item;
        }



        public bool IsDirectory()
        {
            return Type == WorkspaceItemType.Directory;
        }

        public bool IsPlainText()
        {
            switch (Type)
            {
                case WorkspaceItemType.Text:
                case WorkspaceItemType.Latex:
                case WorkspaceItemType.Markdown:
                    return true;
            }
            return false;
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

        private static IEnumerable<WorkspaceItem> FromDirectory(DirectoryInfo directoryInfo, WorkspaceItem parent)
        {
            foreach (var dir in directoryInfo.EnumerateFileSystemInfos())
            {
                yield return new WorkspaceItem(dir, true) { Parent = parent };
            }
        }
    }
}