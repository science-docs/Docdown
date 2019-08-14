using Docdown.Util;
using System;
using System.Collections.Generic;
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

        public string FullPath => System.IO.Path.Combine(Path, ".dc");

        private Dictionary<string, string> properties = new Dictionary<string, string>();

        private WorkspaceSettings(string path)
        {
            Path = path;
        }

        public static WorkspaceSettings Create(string workspacePath)
        {
            var settings = new WorkspaceSettings(workspacePath);
            if (!System.IO.File.Exists(settings.FullPath))
            {
                return settings;
            }

            IEnumerable<Tuple<string, string>> tuples = IOUtility.ParseProperties(settings.FullPath);
            settings.properties = tuples.ToDictionary(e => e.Item1, e => e.Item2);

            return settings;
        }

        public void Save()
        {
            IOUtility.WriteProperties(FullPath, properties
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
