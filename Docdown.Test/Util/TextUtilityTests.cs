using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Docdown.Util.Test
{
    [TestClass]
    public class TextUtilityTests
    {
        [TestMethod]
        public void DefaultIncreaseTest()
        {
            Assert.AreEqual("g", TextUtility.Increase('f'));
        }

        [TestMethod]
        public void NullCharIncreaseTest()
        {
            Assert.IsNull(TextUtility.Increase('z'));
        }

        [TestMethod]
        public void NullSpaceIncreaseTest()
        {
            Assert.IsNull(TextUtility.Increase(' '));
        }

        [TestMethod]
        public void NullNumberIncreaseTest()
        {
            Assert.IsNull(TextUtility.Increase('5'));
        }
    }
}
