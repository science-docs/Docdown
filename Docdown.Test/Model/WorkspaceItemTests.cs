using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO.Abstractions;
using System.Text;
using System.Threading.Tasks;

namespace Docdown.Model.Test
{
    [TestClass]
    public class WorkspaceItemTests : WorkspaceTestBase
    {
        const string MainFullName = @"C:\Workspace\Main.md";
        const string RenamedFullName = @"C:\Workspace\Renamed.md";

        [TestMethod]
        public async Task ConvertTest()
        {
            var main = FindItem("Main.md");
            var path = await main.Convert(null);
            Assert.IsNotNull(path, "Could not create file from converter service");
        }

        [TestMethod]
        public async Task LoadStringTest()
        {
            var main = FindItem("Main.md");
            var bytes = await main.Read();
            Assert.IsNotNull(bytes, "Reading a file should never return NULL");
            Assert.IsTrue(bytes.Length > 0, "This file is specified as having more than zero bytes");
            string text = Encoding.UTF8.GetString(bytes);
            Assert.IsTrue(text.StartsWith("---"), "Text does not start with '---', found instead '{0}'", text.Substring(0, 10));
        }

        [TestMethod]
        public async Task SaveTest()
        {
            var main = FindItem("Main.md");
            const string text = "# Only Header";
            await main.Save(text);
            var newText = FileSystem.File.ReadAllText(MainFullName);
            Assert.AreEqual(text, newText, false, "Text was not correctly written to file system");
        }

        [TestMethod]
        public async Task DeleteTest()
        {
            var main = FindItem("Main.md");
            await main.Delete();
            Assert.IsNull(main.Parent);
            Assert.AreSame(main, main.TopParent);
            Assert.IsFalse(Workspace.Item.Children.Contains(main));
            Assert.IsFalse(FileSystem.FileExists(MainFullName));
        }

        [TestMethod]
        public async Task RenameTest()
        {
            var main = FindItem("Main.md");
            const string newName = "Renamed.md";
            await main.Rename(newName);
            Assert.IsFalse(FileSystem.FileExists(MainFullName));
            Assert.IsTrue(FileSystem.FileExists(RenamedFullName));
        }

        [TestMethod]
        public async Task UpdateTest()
        {
            var main = FindItem("Main.md");
            var fullName = main.FullName;
            await main.Update();
            Assert.AreEqual(fullName, main.FullName);
        }

        [TestMethod]
        public void DirectoryFileTest()
        {
            var main = FindItem("Main.md");
            Assert.IsTrue(main.IsFile);
            Assert.IsFalse(main.IsDirectory);
            Assert.IsInstanceOfType(main.FileInfo, typeof(IFileInfo));
            Assert.IsTrue(Workspace.Item.Children.Contains(main));
            Assert.IsTrue(Workspace.Item.IsDirectory);
            Assert.IsFalse(Workspace.Item.IsFile);
            Assert.IsInstanceOfType(Workspace.Item.FileInfo, typeof(IDirectoryInfo));
        }

        [TestMethod]
        public async Task CreateNewFileTest()
        {
            var bytes = Encoding.UTF8.GetBytes("Some Text");
            var item = await Workspace.Item.CreateNewFile("Renamed", ".md", bytes);
            Assert.IsTrue(Workspace.Item.Children.Contains(item));
            Assert.IsTrue(FileSystem.FileExists(RenamedFullName));
        }

        [TestMethod]
        public async Task CreateNewDirectoryTest()
        {
            var dir = await Workspace.Item.CreateNewDirectory("TestDir");
            Assert.IsTrue(Workspace.Item.Children.Contains(dir));
            Assert.IsTrue(FileSystem.Directory.Exists(WorkspacePath + "TestDir"));
        }

        [TestMethod]
        public async Task CopyFileTest()
        {
            var item = await Workspace.Item.CopyExistingItem(@"C:\Copy\Test.md");
            Assert.IsTrue(Workspace.Item.Children.Contains(item));
            Assert.IsTrue(FileSystem.FileExists(WorkspacePath + "Test.md"));
        }

        [TestMethod]
        public async Task CopyFolderTest()
        {
            var dir = await Workspace.Item.CopyExistingFolder(@"C:\Copy");
            Assert.IsTrue(Workspace.Item.Children.Contains(dir), "Workspace does not contain item");
            Assert.IsTrue(FileSystem.Directory.Exists(WorkspacePath + "Copy"), "Directory was not correctly copied");
            Assert.IsTrue(FileSystem.FileExists(WorkspacePath + @"Copy\Test.md"), "File was not correctly copied");
        }
    }
}