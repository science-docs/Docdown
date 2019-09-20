using Docdown.Util;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;

namespace Docdown.Model
{
    public class WorkspaceSettings
    {
        public string Sync
        {
            get => GetProp("sync");
            set => properties["sync"] = value;
        }
        public string Path { get; set; }

        public IFileSystem FileSystem { get; set; }

        public string FullPath => System.IO.Path.Combine(Path, ".dc");

        private Dictionary<string, string> properties = new Dictionary<string, string>();

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

            IEnumerable<Tuple<string, string>> tuples = IOUtility.ParseProperties(fileSystem.File, settings.FullPath);
            settings.properties = tuples.ToDictionary(e => e.Item1, e => e.Item2);

            return settings;
        }

        public void Save()
        {
            IOUtility.WriteProperties(FileSystem.File, FullPath, properties
                .Where(e => e.Value != null)
                .Select(e => new Tuple<string, string>(e.Key, e.Value)));
        }

        private string GetProp(string name)
        {
            if (properties.TryGetValue(name, out var value))
            {
                return value;
            }
            return null;
        }
    }
}
