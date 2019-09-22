using Docdown.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Security.AccessControl;
using System.Xml.Linq;

namespace Docdown.Model
{
    public class WorkspaceSettings
    {
        public string Path { get; set; }

        public IFileSystem FileSystem { get; set; }

        public string Directory => System.IO.Path.Combine(Path, ".dc");

        public string FullPath => System.IO.Path.Combine(Directory, "settings.xml");

        public List<WorkspaceItemPersistance> Items { get; } = new List<WorkspaceItemPersistance>();

        private WorkspaceSettings(IFileSystem fileSystem, string path)
        {
            Path = path;
            FileSystem = fileSystem;
        }

        public static WorkspaceSettings Create(IFileSystem fileSystem, string workspacePath)
        {
            var settings = new WorkspaceSettings(fileSystem, workspacePath);
            if (!fileSystem.File.Exists(settings.FullPath))
            {
                return settings;
            }

            var fullText = fileSystem.File.ReadAllText(settings.FullPath);
            var doc = XDocument.Parse(fullText);
            settings.Parse(doc);

            return settings;
        }

        private void Parse(XDocument doc)
        {
            var root = doc.Root;
            foreach (var node in root.Elements(nameof(WorkspaceItemPersistance)))
            {
                Items.Add(WorkspaceItemPersistance.Parse(node));
            }
        }

        public void Save()
        {
            try
            {
                FileSystem.Directory.CreateDirectory(Directory);
                FileSystem.File.WriteAllText(FullPath, CreateDocument().ToString());
            }
            catch (Exception e)
            {
                Trace.WriteLine("Could not save workspace settings: " + e.Message);
            }
        }

        private XDocument CreateDocument()
        {
            return new XDocument(new XElement("Settings", Items.Select(e => e.ToXml())));
        }
    }
}
