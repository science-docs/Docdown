using Docdown.Controls;
using Docdown.Properties;
using Docdown.Util;
using Docdown.ViewModel;
using Docdown.Windows;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Docdown
{
    public partial class MainWindow
    {
        readonly AppViewModel app;
        bool closeFlag = true;

        public static ViewWrapper Outline;
        public static ViewWrapper Explorer;

        public MainWindow()
        {
            //if (Debugger.IsAttached)
            //    Settings.Default.Reset();

            var splash = new SplashWindow();
            if (splash.ShowDialog().Value)
            {
                InitializeComponent();
                if (Settings.Default.WasMaximized)
                {
                    WindowState = WindowState.Maximized;
                }
                else
                {
                    Width = Settings.Default.AppWidth;
                    Height = Settings.Default.AppHeight;
                }
                DataContext = app = splash.ViewModel.Data;
                Closing += OnClosing;

                Outline = _Outline;
                Explorer = _Explorer;

                TreeNode c = new TreeNode
                {
                    Element = Explorer
                };

                TreeNode d = new TreeNode
                {
                    Element = Outline
                };



                TreeNode a = new TreeNode
                {
                    Element = _Tab
                };

                TreeNode b = new TreeNode
                {
                    A = c,
                    B = d,
                    Distribution = 0.6,
                    Orientation = System.Windows.Controls.Orientation.Vertical
                };

                c.Parent = d.Parent = b;


                TreeNode node = new TreeNode
                {
                    A = b,
                    B = a,
                    Distribution = 0.2,
                    Orientation = System.Windows.Controls.Orientation.Horizontal
                };

                b.Parent = a.Parent = node;

                app.ContentTree = node;
            }
            else
            {
                Close();
            }
        }

        private Rectangle CreateRect(Brush brush)
        {
            return new Rectangle
            {
                Fill = brush
            };
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = closeFlag;
            if (closeFlag)
            {
                var newArgs = new CancelEventArgs();
                Task.Run(async () =>
                {
                    await app.OnClosing(newArgs);
                    if (!newArgs.Cancel)
                    {
                        closeFlag = false;
                        Dispatcher.Invoke(Close);
                    }
                });
            }
            else
            {
                Settings.Default.AppWidth = (int)ActualWidth;
                Settings.Default.AppHeight = (int)ActualHeight;
                Settings.Default.WasMaximized = WindowState == WindowState.Maximized;
                Settings.Default.Save();
            }
        }

        private void MenuItemExitClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MarkdownHelpMenuClicked(object sender, RoutedEventArgs e)
        {
            ProgramUtility.ExecuteNonWaiting("https://pandoc.org/MANUAL.html#pandocs-markdown");
        }

        private void AboutHelpMenuClicked(object sender, RoutedEventArgs e)
        {
            ProgramUtility.ExecuteNonWaiting("https://github.com/Darkgaja/Docdown");
        }
    }
}