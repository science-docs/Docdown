using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Docdown.Util.Test
{
    [TestClass]
    public class ThesaurusUtilityTests
    {
        [TestMethod]
        public async Task DefaultSynonymTest()
        {
            await AssertSynonyms("Test", "Experiment", "Klausur");
        }

        [TestMethod]
        public async Task EncodedSynonymTest()
        {
            await AssertSynonyms("hören", "lauschen", "horchen", "folgen");
        }

        public async Task AssertSynonyms(string word, params string[] words)
        {
            var synonyms = await ThesaurusUtility.FindSynonym(word);
            foreach (var item in words)
            {
                CollectionAssert.Contains(synonyms, item, "Synonyms of {1} do not contain: {0}", word, item);
            }
        }
    }
}
