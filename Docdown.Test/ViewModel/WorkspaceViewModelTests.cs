using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Docdown.ViewModel.Test
{
    [TestClass]
    public class WorkspaceViewModelTests : WorkspaceViewModelTestBase
    {
        [TestMethod]
        public void CreationTest()
        {
            Assert.AreEqual(WorkspaceVM.Data, Workspace);
        }

        [TestMethod]
        public async Task ConvertResultTest()
        {
            await WorkspaceVM.Convert();
            Assert.IsNotNull(WorkspaceVM.PdfPath);
        }

        [TestMethod]
        public async Task ConvertPropertiesTest()
        {
            ConverterService.Delay = 100;

            Assert.IsFalse(WorkspaceVM.IsConverting);
            Assert.IsFalse(WorkspaceVM.IsCompiled);
            Assert.IsTrue(WorkspaceVM.CanConvert);

            // The assert statements execute during the convert call
            #pragma warning disable CS4014
            Task.Delay(50).ContinueWith(t =>
            {
                Assert.IsTrue(WorkspaceVM.IsConverting);
                Assert.IsFalse(WorkspaceVM.IsCompiled);
                Assert.IsFalse(WorkspaceVM.CanConvert);
            });

            #pragma warning restore CS4014
            await WorkspaceVM.Convert();

            Assert.IsFalse(WorkspaceVM.IsConverting);
            Assert.IsTrue(WorkspaceVM.IsCompiled);
            Assert.IsTrue(WorkspaceVM.CanConvert);

            //Assert.IsNotNull(WorkspaceVM.SelectedPreview);
            //Assert.IsInstanceOfType(WorkspaceVM.SelectedPreview.Data, typeof(PreviewWorkspaceItem));
            //Assert.AreEqual("Workspace [Preview]", WorkspaceVM.SelectedPreview.Name);
        }
    }
}
