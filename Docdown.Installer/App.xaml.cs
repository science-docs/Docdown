using MahApps.Metro;
using System;
using System.Windows;

namespace Docdown.Installer
{
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            ThemeManager.AddAccent("BlueDoc", GetResourceUri("Resources/Blue.xaml"));
            ThemeManager.AddAppTheme("DarkDoc", GetResourceUri("Resources/Dark.xaml"));
            ThemeManager.ChangeAppStyle(Current, ThemeManager.GetAccent("BlueDoc"), ThemeManager.GetAppTheme("DarkDoc"));
            base.OnStartup(e);
        }

        public static Uri GetResourceUri(string resource)
        {
            return new Uri("pack://application:,,,/Docdown.Installer;component/" + resource);
        }
    }
}
