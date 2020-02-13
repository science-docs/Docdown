using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Docdown.Bibliography.Test
{
    [TestClass]
    public class SciHubLoaderTests
    {
        [TestMethod]
        public async Task FindSciHubTest()
        {
            Assert.IsNull(SciHubLoader.CurrentSciHubAddress);
            string url = await SciHubLoader.FindAddress();
            Assert.IsTrue(url.StartsWith("https://sci-hub."));
            Assert.AreEqual(url, SciHubLoader.CurrentSciHubAddress);
        }
    }
}
