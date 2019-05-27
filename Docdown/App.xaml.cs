using Docdown.Properties;
using Docdown.ViewModel;
using MahApps.Metro;
using System;
using System.Windows;

namespace Docdown
{
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            ThemeManager.AddAccent("BlueDoc", GetResourceUri("Resources/Accents/Blue.xaml"));
            ThemeManager.AddAppTheme("DarkDoc", GetResourceUri("Resources/Themes/Dark.xaml"));
            ThemeManager.AddAppTheme("LightDoc", GetResourceUri("Resources/Themes/Light.xaml"));
            var theme = Settings.Default.Theme;
            ChangeLocale(Settings.Default.Locale);
            ThemeManager.ChangeAppStyle(Current, ThemeManager.GetAccent("BlueDoc"), ThemeManager.GetAppTheme(theme + "Doc"));
            SplashViewModel.Args = e.Args;
            base.OnStartup(e);
        }

        public static Uri GetResourceUri(string resource)
        {
            return new Uri("pack://application:,,,/Docdown;component/" + resource);
        }

        public static void ReloadIcons()
        {
            LoadResource(GetResourceUri("Resources/Images/Icons.xaml"));
        }

        public static void ChangeLocale(string locale)
        {
            LoadResource(GetResourceUri($"Resources/Locale/{locale}.xaml"));
        }

        private static void LoadResource(Uri uri)
        {
            var resourceDictionary = new ResourceDictionary
            {
                Source = uri
            };
            Current.Resources.MergedDictionaries.Remove(resourceDictionary);
            Current.Resources.MergedDictionaries.Add(resourceDictionary);
        }
    }
}