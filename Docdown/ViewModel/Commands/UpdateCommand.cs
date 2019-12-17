using Docdown.Util;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace Docdown.ViewModel.Commands
{
    public class UpdateCommand : DelegateCommand
    {
        public UpdateCommand(IProgress<WebDownloadProgress> progress) : base(progress)
        {

        }

        [Delegate]
        public async Task Update(IProgress<WebDownloadProgress> progress)
        {
            progress = progress ?? new Progress<WebDownloadProgress>();
            var newVersion = await UpdateUtility.CheckNewVersion();
            var app = AppViewModel.Instance;
            var dispatcher = app.Dispatcher;
            if (newVersion != null && await dispatcher.InvokeAsync(() => Util.MessageBox.Show("New Version available",
                $"Do you want to download version {newVersion} now?", MessageBoxButton.YesNo)) == MessageBoxResult.Yes)
            {
                app.Messages.Add("Updating...", MessageType.Working);
                await UpdateUtility.Update(progress);
            }
        }
    }
}
