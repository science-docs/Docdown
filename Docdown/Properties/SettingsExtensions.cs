using System;
using System.Linq;
using System.Configuration;

namespace Docdown.Properties
{
    public static class SettingsExtensions
    {
        private static readonly SettingsAttributeDictionary Attributes = new SettingsAttributeDictionary()
        {
            { typeof(UserScopedSettingAttribute), new UserScopedSettingAttribute() }
        };

        public static bool EnsureProperty<T>(this Settings settings, string name, T defaultValue = default)
        {
            if (settings is null)
                throw new ArgumentNullException(nameof(settings));
            if (name is null)
                throw new ArgumentNullException(nameof(name));

            var type = typeof(T);
            if (!settings.ContainsProperty(name, type))
            {
                var provider = settings.Providers.OfType<SettingsProvider>().First();
                var property = new SettingsProperty(name, type, provider, false, defaultValue, SettingsSerializeAs.String, Attributes, false, false);
                settings.Properties.Add(property);
                return false;
            }
            return true;
        }

        public static bool ContainsProperty(this Settings settings, string name, Type type)
        {
            if (settings is null)
                throw new ArgumentNullException(nameof(settings));
            if (name is null)
                throw new ArgumentNullException(nameof(name));
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            var props = settings.Properties;
            return props[name] != null && props[name].PropertyType == type;
        }
    }
}
