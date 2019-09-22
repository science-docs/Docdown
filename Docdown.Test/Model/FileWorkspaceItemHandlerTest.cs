using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Docdown.Model.Test
{
    [TestClass]
    public class FileWorkspaceItemHandlerTest : WorkspaceTestBase
    {
        [TestMethod]
        public void ReadTest()
        {
            var item = FindItem("Main.md");
            var bytes = FileHandler.Read(item);
            CollectionAssert.AreEqual(bytes, FileSystem.File.ReadAllBytes(WorkspacePath + "Main.md"));
        }
    }
}
