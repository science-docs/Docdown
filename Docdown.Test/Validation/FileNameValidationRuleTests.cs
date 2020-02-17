using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;

namespace Docdown.Validation.Test
{
    [TestClass]
    public class FileNameValidationRuleTests : ValidationRuleTestBase<FileNameValidationRule>
    {
        [TestMethod]
        public void EmptyFileNameTest()
        {
            var result = Rule.Validate(string.Empty, CultureInfo.CurrentCulture);
            Assert.IsFalse(result.IsValid);
        }

        [TestMethod]
        public void InvalidCharactersTest()
        {
            var result = Rule.Validate("?", CultureInfo.CurrentCulture);
            Assert.IsFalse(result.IsValid);
            Assert.IsInstanceOfType(result.ErrorContent, typeof(string));
            var content = result.ErrorContent as string;
            Assert.IsTrue(content.Contains("?"));
        }

        [TestMethod]
        public void ValidCharactersTest()
        {
            var result = Rule.Validate("Test File_Name.txt", CultureInfo.CurrentCulture);
            Assert.IsTrue(result.IsValid);
        }
    }
}
