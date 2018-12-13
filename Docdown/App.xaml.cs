using MahApps.Metro;
using System;
using System.Windows;

namespace Docdown
{
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            ThemeManager.AddAccent("BluePPI",
                new Uri("pack://application:,,,/Docdown;component/Resources/BluePPI.xaml"));
            Tuple<AppTheme, Accent> theme = ThemeManager.DetectAppStyle(Current);
            ThemeManager.ChangeAppStyle(Current, ThemeManager.GetAccent("BluePPI"), theme.Item1);
            
            base.OnStartup(e);
        }
    }
}
