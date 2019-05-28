using Docdown.Model;
using Docdown.Properties;
using Docdown.Util;
using Docdown.ViewModel.Commands;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Docdown.ViewModel
{
    public class SplashViewModel : ObservableObject<WorkspaceViewModel>
    {
        public static string[] Args { get; internal set; }

        private readonly Action<bool?> setDialogResult;

        public SplashViewModel(Action<bool?> setDialogResult) : base(null)
        {
            this.setDialogResult = setDialogResult;
        }

        public void Initialize()
        {
            var settings = Settings.Default;
            var workspacePath = settings.WorkspacePath;
            if (Args != null && Args.Length > 0 && File.Exists(Args[0]) && !IOUtility.IsParent(workspacePath, Args[0]))
            {
                workspacePath = Path.GetDirectoryName(Args[0]);
            }
            else if (string.IsNullOrEmpty(workspacePath) || !Directory.Exists(workspacePath))
            {
                workspacePath = new SearchFolderCommand(null, "Select workspace").ExecuteWithResult();
            }
            if (workspacePath != null)
            {
                Task.Run(() =>
                {
                    settings.WorkspacePath = workspacePath;
                    settings.Save();

                    var workspace = WorkspaceProvider.Create(new Uri(workspacePath));
                    workspace.ToType = ConverterType.Pdf;
                    var workspaceViewModel = new WorkspaceViewModel(workspace);
                    workspaceViewModel.Settings.TestConnection();

                    Data = workspaceViewModel;

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
