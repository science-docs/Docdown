using System.Collections.Generic;
using System.Configuration;

namespace Docdown.Settings.Test
{
    public class MemorySettingsProvider : SettingsProvider
    {
        public override string ApplicationName { get; set; } = "Docdown";

        private readonly Dictionary<string, object> values = new Dictionary<string, object>();

        public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext context, SettingsPropertyCollection collection)
        {
            var valueCollection = new SettingsPropertyValueCollection();
            foreach (SettingsProperty prop in collection)
            {
                object value;
                if (values.ContainsKey(prop.Name))
                {
                    value = values[prop.Name];
                }
                else
                {
                    value = prop.DefaultValue;
                }
                valueCollection.Add(new SettingsPropertyValue(prop) { PropertyValue = value });
            }

            return valueCollection;
        }

        public override void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection collection)
        {
            foreach (SettingsPropertyValue value in collection)
            {
                values[value.Name] = value.PropertyValue;
            }
        }
    }
}
