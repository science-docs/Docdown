using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Threading.Tasks;

namespace Docdown.Model.Test
{
    [TestClass]
    public class WorkspaceTests : WorkspaceTestBase
    {
        const string NewName = "C:/Workspace/New.txt";

        [TestMethod]
        public async Task FileCreatedTest()
        {
            FileSystem.File.Create(NewName);
            Watcher.Trigger(new FileSystemEventArgs(WatcherChangeTypes.Created, "C:/Workspace", "New.txt"));
            await Task.Delay(1100);
            var newItem = FindItem("New.txt");
            Assert.IsNotNull(newItem);
        }
    }
}