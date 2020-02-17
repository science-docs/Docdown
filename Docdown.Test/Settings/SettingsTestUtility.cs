using System.Configuration;
using System.Linq;

namespace Docdown.Settings.Test
{
    public static class SettingsTestUtility
    {
        public static void UseMemorySettingsProvider()
        {
            var settings = Properties.Settings.Default;
            if (!settings.Providers.OfType<MemorySettingsProvider>().Any())
            {
                var memProvider = new MemorySettingsProvider();
                var fileProvider = settings.Providers[nameof(LocalFileSettingsProvider)];
                var context = new SettingsContext();
                memProvider.Initialize(nameof(MemorySettingsProvider), new System.Collections.Specialized.NameValueCollection());
                memProvider.SetPropertyValues(context, fileProvider.GetPropertyValues(context, settings.Properties));
                settings.Providers.Remove(nameof(LocalFileSettingsProvider));
                settings.Providers.Add(memProvider);
            }
        }
    }
}
