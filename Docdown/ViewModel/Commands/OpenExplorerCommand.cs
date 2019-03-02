using System;
using System.Diagnostics;
using System.IO;

namespace Docdown.ViewModel.Commands
{
    public class OpenExplorerCommand : DelegateCommand
    {
        public OpenExplorerCommand(string path) : base(path)
        {

        }

        [Delegate]
        private static void Open(string path)
        {
            if (Directory.Exists(path))
            {
                Process.Start(path);
            }
        }
    }
}