using Docdown.Properties;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Configuration;

namespace Docdown.Settings.Test
{
    [TestClass]
    public class SettingsExtensionsTests
    {
        const string PropertyName = "SomeProperty";
        const string ExistingProperty = "Theme";
        const string DefaultValue = "SomeValue";
        const string OtherValue = "SomeOtherValue";

        private Properties.Settings Item => Properties.Settings.Default;

        [TestInitialize]
        public void InitializeTests()
        {
            SettingsTestUtility.UseMemorySettingsProvider();
        }

        [TestMethod]
        public void ContainsPropertyTest()
        {
            Assert.IsTrue(Item.ContainsProperty(ExistingProperty, typeof(string)));
        }

        [TestMethod]
        public void ContainsPropertyWrongTypeTest()
        {
            Assert.IsFalse(Item.ContainsProperty(ExistingProperty, typeof(int)));
        }

        [TestMethod]
        public void ContainsPropertyWrongNameTest()
        {
            Assert.IsFalse(Item.ContainsProperty(PropertyName, typeof(string)));
        }

        [TestMethod]
        public void ContainsPropertyExceptionTest()
        {
            var nullSettings = Assert.ThrowsException<ArgumentNullException>(() => SettingsExtensions.ContainsProperty(null, PropertyName, typeof(bool)));
            Assert.AreEqual("settings", nullSettings.ParamName);

            var nullName = Assert.ThrowsException<ArgumentNullException>(() => Item.ContainsProperty(null, typeof(bool)));
            Assert.AreEqual("name", nullName.ParamName);
            var nullType = Assert.ThrowsException<ArgumentNullException>(() => Item.ContainsProperty(PropertyName, null));
            Assert.AreEqual("type", nullType.ParamName);
        }

        [TestMethod]
        public void EnsurePropertyTest()
        {
            // First ensure that the property does not exist
            Assert.ThrowsException<SettingsPropertyNotFoundException>(() => Item[PropertyName]);
            // Add the property to the settings
            Assert.IsFalse(Item.EnsureProperty(PropertyName, DefaultValue));
            Assert.AreEqual(DefaultValue, Item[PropertyName]);
            // Ensure that the method is an idempotent action
            Assert.IsTrue(Item.EnsureProperty(PropertyName, OtherValue));
            Assert.AreEqual(DefaultValue, Item[PropertyName]);
        }

        [TestMethod]
        public void EnsurePropertyExceptionTest()
        {
            var nullSettings = Assert.ThrowsException<ArgumentNullException>(() => SettingsExtensions.EnsureProperty<bool>(null, PropertyName));
            Assert.AreEqual("settings", nullSettings.ParamName);

            var nullName = Assert.ThrowsException<ArgumentNullException>(() => Item.EnsureProperty<bool>(null));
            Assert.AreEqual("name", nullName.ParamName);
        }
    }
}
