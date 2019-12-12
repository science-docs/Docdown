using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Docdown.Installer
{
    public class Model
    {
        public bool StartupShortcut { get; set; } = true;
        public bool DesktopShortcut { get; set; } = true;

        public bool MdFileAssociation { get; set; } = true;
        public bool BibFileAssociation { get; set; } = true;

        public ICommand InstallCommand => new InternalInstallCommand(this);

        public async Task Install()
        {
            const string name = "Docdown";
            var userFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var docTarget = Path.Combine(userFolder, name, "Docdown.exe");
            var names = typeof(Model).Assembly.GetManifestResourceNames();
            await Util.Extract("Docdown.Installer.Resources.Docdown.exe", docTarget);
            try
            {
                if (StartupShortcut)
                {
                    Shortcuts.AddStartMenuShortcut(docTarget, name);
                }
                if (DesktopShortcut)
                {
                    Shortcuts.AddDesktopShortcut(docTarget, name);
                }
                if (MdFileAssociation)
                {
                    FileAssociations.EnsureAssociationsSet(".md", "Markdown File");
                }
                if (BibFileAssociation)
                {
                    FileAssociations.EnsureAssociationsSet(".bib", "Bibliography File");
                }
                Process.Start(docTarget);
                Application.Current.Dispatcher.Invoke(() => Application.Current.MainWindow.Close());
            }
            catch (Exception e)
            {

            }
            
        }

        private class InternalInstallCommand : ICommand
        {
            public event EventHandler CanExecuteChanged;

            public Model Model { get; set; }

            public InternalInstallCommand(Model model)
            {
                Model = model;
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
                Task.Run(Model.Install);
            }
        }
    }
}
