using System;
using System.Collections.Generic;
using System.IO;

namespace Docdown.Model
{
    public enum WorkspaceItemType
    {
        Other = 0,
        Markdown = 1,
        Latex = 2,
        Pdf = 3,
        Directory = 4
    }

    public class WorkspaceItem
    {
        public FileSystemInfo FileSystemInfo { get; set; }
        public WorkspaceItemType Type { get; set; }
        public List<WorkspaceItem> Children { get; } = new List<WorkspaceItem>();

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
                    Children.AddRange(FromDirectory(directoryInfo));
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
                    default:
                        Type = WorkspaceItemType.Other;
                        break;
                }
            }
        }

        public WorkspaceItem CreateNewMarkdownChild(string name, string content = "")
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("message", nameof(name));
            if (Type != WorkspaceItemType.Directory)
                throw new InvalidOperationException("Only Directories can contain files");

            string fullName = Path.Combine(FileSystemInfo.FullName, name);

            File.WriteAllText(fullName, content);
            var fileInfo = new FileInfo(fullName);
            var item = new WorkspaceItem(fileInfo);
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

        public bool IsPlainText()
        {
            return Type != WorkspaceItemType.Directory;
            switch (Type)
            {
                case WorkspaceItemType.Latex:
                case WorkspaceItemType.Markdown:
                    return true;
            }
            return false;
        }
        
        private static IEnumerable<WorkspaceItem> FromDirectory(DirectoryInfo directoryInfo)
        {
            foreach (var dir in directoryInfo.EnumerateFileSystemInfos())
            {
                yield return new WorkspaceItem(dir, true);
            }
        }
    }
}