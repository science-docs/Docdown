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
            ThemeManager.ChangeAppStyle(Current, ThemeManager.GetAccent("BlueDoc"), ThemeManager.GetAppTheme("DarkDoc"));
            
            base.OnStartup(e);
        }

        public static Uri GetResourceUri(string resource)
        {
            return new Uri("pack://application:,,,/Docdown;component/" + resource);
        }
    }
}