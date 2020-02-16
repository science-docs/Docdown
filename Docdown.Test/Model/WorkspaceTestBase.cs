using Docdown.Editor.Markdown;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PandocMark.Syntax;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Docdown.Model.Test
{
    public abstract class WorkspaceTestBase : TestBase
    {
        public IWorkspace Workspace { get; set; }
        public MockConverterService ConverterService { get; set; }
        public MockFileSystem FileSystem { get; set; }
        public FileWorkspaceItemHandler FileHandler { get; set; }
        public FileSystemWatcherFactory Watcher { get; set; }
        public const string WorkspacePath = @"C:\Workspace\";

        [TestInitialize]
        public async Task Init()
        {
            await Initialize();
        }

        public static void AssertParentChild(IWorkspaceItem parent, IWorkspaceItem child)
        {
            CollectionAssert.Contains(parent.Children, child, "Item '{0}' does not contain '{1}'", parent, child);
            Assert.AreSame(parent, child.Parent, "Item '{0}' is not the parent of '{1}'", parent, child);
        }

        public async Task Initialize()
        {
            const string prefix = "Docdown.Test.Files.";
            var asm = Assembly.GetExecutingAssembly();
            var names = asm.GetManifestResourceNames().Where(e => e.StartsWith(prefix));

            var dict = new Dictionary<string, MockFileData>();

            foreach (var name in names)
            {
                var fileName = name.Substring(prefix.Length, name.Length - prefix.Length).Replace('.', '\\');
                var lastIndex = fileName.LastIndexOf('\\');
                fileName = fileName.Remove(lastIndex, 1).Insert(lastIndex, ".").Replace('_', '.');
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

            FileSystem = new MockFileSystem(dict, @"C:\")
            {
                FileSystemWatcher = Watcher = new FileSystemWatcherFactory()
            };

            ConverterService = new MockConverterService();
            Workspace = await WorkspaceProvider.Create(WorkspacePath, FileSystem, ConverterService);
            Workspace.WorkspaceChanged += delegate { };
            FileHandler = Workspace.Handlers.OfType<FileWorkspaceItemHandler>().First();
        }

        public IWorkspaceItem FindItem(string name)
        {
            return FindItem(Workspace.Item, name);
        }

        private IWorkspaceItem FindItem(IWorkspaceItem item, string name)
        {
            if (!item.IsDirectory)
            {
                return null;
            }

            foreach (var child in item.Children)
            {
                if (child.Name == name)
                {
                    return child;
                }
                var childItem = FindItem(child, name);
                if (childItem != null)
                {
                    return childItem;
                }
            }
            return null;
        }

        public static IEnumerable<Issue> Validate(IWorkspaceItem item)
        {
            var text = Encoding.UTF8.GetString(item.Read());
            return MarkdownValidator.Validate(text, item);
        }
    }
}
