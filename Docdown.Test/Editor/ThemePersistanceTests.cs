using Docdown.Settings.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Docdown.Editor.Test
{
    [TestClass]
    public class ThemePersistanceTests
    {
        [TestInitialize]
        public void SetMemorySettings()
        {
            SettingsTestUtility.UseMemorySettingsProvider();
        }

        [TestMethod]
        public void MarkdownDefaultTest()
        {
            var theme = ThemePersistance.Load("Markdown");
            Assert.IsNotNull(theme);
            Assert.AreEqual("Markdown", theme.Name);
            CollectionAssert.AllItemsAreNotNull(theme.Highlights.ToArray());
        }

        [TestMethod]
        public void SaveLoadTest()
        {
            var theme = ThemePersistance.Load("Markdown");
            const string NewName = "TestMarkdown";
            const string FirstBackground = "#00000000";
            theme.Name = NewName;
            theme.Highlights.First().Background = FirstBackground;

            ThemePersistance.Save(theme);
            var loaded = ThemePersistance.Load(NewName);

            Assert.AreEqual(theme.Name, loaded.Name);
            Assert.AreEqual(NewName, loaded.Name);

            Assert.AreEqual(FirstBackground, loaded.Highlights.First().Background);
        }
    }
}
