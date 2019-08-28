using Docdown.Editor.Markdown;
using Docdown.Model;
using PandocMark.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Docdown.Test.Model
{
    public abstract class WorkspaceTestBase
    {
        public IWorkspace Workspace { get; set; }
        public MockFileSystem FileSystem { get; set; }

        public async Task Initialize()
        {
            const string prefix = "Docdown.Test.Files.";
            var asm = Assembly.GetExecutingAssembly();
            var names = asm.GetManifestResourceNames().Where(e => e.StartsWith(prefix));

            var dict = new Dictionary<string, MockFileData>();

            foreach (var name in names)
            {
                var fileName = name.Substring(prefix.Length, name.Length - prefix.Length).Replace('.', '\\');
                byte[] bytes;
                using (var stream = asm.GetManifestResourceStream(name))
                {
                    using (var ms = new MemoryStream())
                    {
                        stream.CopyTo(ms);
                        bytes = ms.ToArray();
                    }
                }
                dict.Add(fileName, new MockFileData(bytes));
            }

            FileSystem = new MockFileSystem(dict, @"C:\");

            Workspace = await WorkspaceProvider.Create(@"C:\Workspace", FileSystem);
        }

        public static IEnumerable<Issue> Validate(IWorkspaceItem item)
        {
            var text = Encoding.UTF8.GetString(item.Read().Result);
            return MarkdownValidator.Validate(text, item);
        }
    }
}
