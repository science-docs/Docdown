using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Docdown.Util.Test
{
    [TestClass]
    public class BibliographyUtilityTests
    {
        [TestMethod]
        public async Task DOITest()
        {
            string bib = await BibliographyUtility.SearchBibliographyEntry("10.5555/12345678");
            Assert.IsNotNull(bib);
            Assert.IsTrue(bib.Contains("Toward a Unified Theory of High-Energy Metaphysics: Silly String Theory"));
        }

        [TestMethod]
        public async Task DOITestDirect()
        {
            string bib = await BibliographyUtility.SearchDOI("10.5555/12345678");
            Assert.IsNotNull(bib);
            Assert.IsTrue(bib.Contains("Toward a Unified Theory of High-Energy Metaphysics: Silly String Theory"));
        }

        [TestMethod]
        public async Task ISBNTest()
        {
            string bib = await BibliographyUtility.SearchBibliographyEntry("9781780648903");
            Assert.IsNotNull(bib);
            Assert.IsTrue(bib.Contains("Climate change and cotton production in modern farming systems"));
        }

        [TestMethod]
        public async Task ISBNDirectTest()
        {

            string bib = await BibliographyUtility.SearchISBN("9781780648903");
            Assert.IsNotNull(bib);
            Assert.IsTrue(bib.Contains("Climate change and cotton production in modern farming systems"));
        }
    }
}
