using Docdown.Model;
using Docdown.Properties;
using Docdown.Util;
using Docdown.ViewModel.Commands;
using Docdown.Windows;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Docdown.ViewModel
{
    public class SplashViewModel : ObservableObject<AppViewModel>
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

            Task.Run(() =>
            {
                var app = new AppViewModel();
                app.Settings.TestConnection();
                app.ChangeWorkspace(workspacePath);

                Data = app;

                setDialogResult?.Invoke(true);
            });
        }
    }
}
