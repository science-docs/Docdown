using Docdown.Model;
using Docdown.Properties;
using Docdown.ViewModel.Commands;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Docdown.ViewModel
{
    public class SplashViewModel : ObservableObject<WorkspaceViewModel>
    {
        private readonly Action<bool?> setDialogResult;

        public SplashViewModel(Action<bool?> setDialogResult) : base(null)
        {
            this.setDialogResult = setDialogResult;
        }

        public void Initialize()
        {
            var settings = Settings.Default;
            var workspacePath = settings.WorkspacePath;
            if (string.IsNullOrEmpty(workspacePath) || !Directory.Exists(workspacePath))
            {
                var command = new SearchWorkspaceCommand(null, path =>
                {
                    workspacePath = path;
                });
                command.Execute();
            }
            if (workspacePath != null)
            {
                Task.Run(() =>
                {
                    settings.WorkspacePath = workspacePath;
                    settings.Save();

                    var workspace = new Workspace(workspacePath)
                    {
                        ToType = ConverterType.Pdf
                    };
                    workspace.LoadTemplates();

                    Data = new WorkspaceViewModel(workspace);

                    setDialogResult?.Invoke(true);
                });
            }
            else
            {
                setDialogResult?.Invoke(false);
            }
        }
    }
}
