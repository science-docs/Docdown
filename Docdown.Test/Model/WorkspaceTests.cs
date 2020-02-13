using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Threading.Tasks;

namespace Docdown.Model.Test
{
    [TestClass]
    public class WorkspaceTests : WorkspaceTestBase
    {
        const string NewName = "C:/Workspace/New.txt";
        const string IgnoredDirectory = "C:/Workspace/.Ignore";
        const string NewHiddenName = "C:/Workspace/.Ignore/New.txt";

        [TestMethod]
        public async Task ConvertTest()
        {
            var path = await Workspace.Convert(null);
            Assert.IsNotNull(path, "Could not create file from converter service");
        }

        [TestMethod]
        public async Task FileCreatedTest()
        {
            FileSystem.File.Create(NewName);
            Watcher.Trigger(new FileSystemEventArgs(WatcherChangeTypes.Created, "C:/Workspace", "New.txt"));
            await Task.Delay(1100);
            var newItem = FindItem("New.txt");
            Assert.IsNotNull(newItem);
        }

        [TestMethod]
        public void HiddenTest()
        {
            var dir = FindItem(".Ignore");
            var item = FindItem("Hidden.txt");
            Assert.IsNull(item);
            Assert.IsNull(dir);
        }

        [TestMethod]
        public async Task HiddenFileCreated()
        {
            FileSystem.Directory.CreateDirectory(IgnoredDirectory);
            FileSystem.File.Create(NewHiddenName);
            Watcher.Trigger(new FileSystemEventArgs(WatcherChangeTypes.Created, IgnoredDirectory, "New.txt"));
            await Task.Delay(1100);
            var newItem = FindItem("New.txt");
            Assert.IsNull(newItem);
        }
    }
}