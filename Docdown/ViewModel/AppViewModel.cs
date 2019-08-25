using Docdown.Editor.Commands;
using Docdown.Model;
using Docdown.ViewModel.Commands;
using Docdown.Windows;
using MahApps.Metro;
using MahApps.Metro.Controls;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Docdown.ViewModel
{
    public class AppViewModel : ObservableObject
    {
        public static AppViewModel Instance { get; private set; }

        public SettingsViewModel Settings { get; }
        public UserViewModel User { get; }
        public new MessageQueue Messages { get; }
        [ChangeListener(nameof(Theme))]
        public string ThemeName => "Lightbulb" + theme;
        public Theme Theme
        {
            get => theme;
            set => Set(ref theme, value);
        }
        public WorkspaceViewModel Workspace { get; set; }

        public bool ExplorerVisible
        {
            get => explorerVisible;
            set => Set(ref explorerVisible, value);
        }

        public bool OutlineVisible
        {
            get => outlineVisible;
            set => Set(ref outlineVisible, value);
        }

        private bool explorerVisible = true;
        private bool outlineVisible = true;

        public ICommand SearchWorkspaceCommand => new SearchFolderCommand(Settings.WorkspacePath, "Select workspace", ChangeWorkspace);
        public ICommand OpenSettingsCommand => new OpenWindowCommand<SettingsWindow>(Settings);
        public ICommand SwitchThemeCommand => new ActionCommand(SwitchTheme);
        public ICommand ChangeDocumentLanguageCommand => new ChangeDocumentLanguageCommand();
        public ICommand ChangeLanguageCommand => new ChangeLanguageCommand();
        public ICommand BoldCommand => new EditCommand(this, EditorCommands.Bold);
        public ICommand ItalicCommand => new EditCommand(this, EditorCommands.Italic);
        public ICommand QuoteCommand => new EditCommand(this, EditorCommands.Quote);
        public ICommand BulletListCommand => new EditCommand(this, EditorCommands.BulletList);
        public ICommand NumberListCommand => new EditCommand(this, EditorCommands.NumberList);

        private Theme theme;

        public AppViewModel()
        {
            if (Instance != null)
            {
                throw new InvalidOperationException("Cannot create a second app view model");
            }
            Instance = this;
            Workspace = new WorkspaceViewModel();
            Settings = new SettingsViewModel(this);
            Messages = new MessageQueue();
            User = new UserViewModel();
            theme = (Theme)Enum.Parse(typeof(Theme), Properties.Settings.Default.Theme);
        }

        public void ChangeWorkspace(string newWorkspace)
        {
            if (string.IsNullOrEmpty(newWorkspace))
                return;

            Settings.WorkspacePath = newWorkspace;
            Settings.Save();
            try
            {
                Task.Run(async () =>
                {
                    var workspace = await WorkspaceProvider.Create(newWorkspace);
                    await Workspace.DataAsync(workspace);
                });
            }
            catch
            {
                // Unable to open workspace
            }
            SendPropertyUpdate(nameof(SearchWorkspaceCommand));
        }

        public void SwitchTheme()
        {
            if (theme == Theme.Dark)
            {
                Theme = Theme.Light;
            }
            else
            {
                Theme = Theme.Dark;
            }
            Properties.Settings.Default.Theme = Theme.ToString();
            Properties.Settings.Default.Save();
            ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.GetAccent("BlueDoc"), ThemeManager.GetAppTheme(theme + "Doc"));
            App.ReloadIcons();
            Workspace.UpdateIcons(Workspace.Children);
        }

        public async Task OnClosing(CancelEventArgs args)
        {
            await Workspace.OnClosing(args);
        }
    }
}
