using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Docdown.ViewModel.Test
{
    [TestClass]
    public class WorkspaceItemViewModelTest : WorkspaceViewModelTestBase
    {
        [TestMethod]
        public void CreationTest()
        {
            Assert.AreEqual(WorkspaceVM.Item.Data, Workspace.Item);
        }

        [TestMethod]
        public void DeepExcludeTest()
        {
            WorkspaceVM.Item.IsExcluded = true;
            var item = FindItem("Main.md");
            Assert.IsTrue(Workspace.Item.IsExcluded);
            Assert.IsTrue(Workspace.Item.IsExcludedEffectively);
            Assert.IsFalse(item.IsExcluded);
            Assert.IsTrue(item.IsExcludedEffectively);
        }
    }
}
