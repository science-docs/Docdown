using Docdown.Model;
using Docdown.Properties;
using Docdown.ViewModel;
using Docdown.ViewModel.Commands;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Docdown.Windows
{
    public partial class SplashWindow
    {
        public Workspace Workspace { get; private set; }

        public SplashWindow()
        {
            InitializeComponent();
            DataContext = new ObservableImpl();
        }

        private void SplashInitialized(object sender, EventArgs e)
        {
            var settings = Settings.Default;
            var workspacePath = settings.WorkspacePath;
            if (string.IsNullOrEmpty(workspacePath) || !Directory.Exists(workspacePath))
            {
                var command = new SearchWorkspaceCommand(null, path =>
                {
                    workspacePath = path;
                });
                command.Execute(null);
            }
            if (workspacePath != null)
            {
                Task.Run(() =>
                {
                    settings.WorkspacePath = workspacePath;
                    settings.Save();

                    Workspace = new Workspace(workspacePath)
                    {
                        ToType = ConverterType.Pdf
                    };
                    Workspace.LoadTemplates();
                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        DialogResult = true;
                    }));
                });
            }
            else
            {
                DialogResult = false;
            }
        }

        private class ObservableImpl : ObservableObject
        {

        }
    }
}