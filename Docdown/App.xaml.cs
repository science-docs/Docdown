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
            ThemeManager.AddAccent("BlueDoc", GetResourceUri("Resources/Themes/Blue.xaml"));
            ThemeManager.AddAppTheme("DarkDoc", GetResourceUri("Resources/Themes/Dark.xaml"));
            ThemeManager.AddAppTheme("LightDoc", GetResourceUri("Resources/Themes/Light.xaml"));
            var theme = Settings.Default.Theme;
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
            var myResourceDictionary = new ResourceDictionary
            {
                Source = GetResourceUri("Resources/Images/VS.xaml")
            };
            Current.Resources.MergedDictionaries.Remove(myResourceDictionary);
            Current.Resources.MergedDictionaries.Add(myResourceDictionary);
        }
    }
}