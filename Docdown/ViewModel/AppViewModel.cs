using Docdown.Controls;
using Docdown.Editor.Commands;
using Docdown.Model;
using Docdown.ViewModel.Commands;
using Docdown.Windows;
using MahApps.Metro;
using MahApps.Metro.Controls;
using System;
using System.ComponentModel;
using System.IO.Abstractions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Docdown.ViewModel
{
    public class AppViewModel : ObservableObject
    {
        public static AppViewModel Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AppViewModel();
                }
                return instance;
            }
        }

        public SettingsViewModel Settings { get; }
        public UserViewModel User { get; }
        public new MessageQueue Messages { get; }
        [ChangeListener(nameof(Theme))]
        public string ThemeName => "Lightbulb" + theme;
        public string Theme
        {
            get => theme;
            set => Set(ref theme, value);
        }
        public WorkspaceViewModel Workspace { get; set; }
        public IConverterService ConverterService { get; set; }
        public IFileSystem FileSystem { get; set; }

        public TreeNode ContentTree { get; set; }

        public ICommand ShowExplorerCommand => new ActionCommand(() => ShowNode(CommonControlType.Explorer));
        public ICommand ShowOutlineCommand => new ActionCommand(() => ShowNode(CommonControlType.Outline));

        public ICommand SearchWorkspaceCommand => new SearchFolderCommand(Settings.WorkspacePath, "Select workspace", ChangeWorkspace);
        public ICommand OpenSettingsCommand => new OpenWindowCommand<SettingsWindow>(Settings);
        public ICommand SwitchThemeCommand => new ActionCommand(SwitchTheme);
        public ICommand UpdateCommand => new UpdateCommand(null);
        public ICommand ChangeDocumentLanguageCommand => new ChangeDocumentLanguageCommand();
        public ICommand ChangeLanguageCommand => new ChangeLanguageCommand();
        public ICommand BoldCommand => new EditCommand(this, EditorCommands.Bold);
        public ICommand ItalicCommand => new EditCommand(this, EditorCommands.Italic);
        public ICommand QuoteCommand => new EditCommand(this, EditorCommands.Quote);
        public ICommand VerbatimCommand => new EditCommand(this, EditorCommands.Verbatim);
        public ICommand CommentCommand => new EditCommand(this, EditorCommands.Comment);
        public ICommand RemoveListCommand => new EditCommand(this, EditorCommands.RemoveList);
        public ICommand BulletListCommand => new EditCommand(this, EditorCommands.BulletList);
        public ICommand DotNumberListCommand => new EditCommand(this, EditorCommands.DotNumberList);
        public ICommand ParenthesisNumberListCommand => new EditCommand(this, EditorCommands.ParenthesisNumberList);
        public ICommand DotAlphabeticalListCommand => new EditCommand(this, EditorCommands.DotAlphabeticalList);
        public ICommand ParenthesisAlphabeticalListCommand => new EditCommand(this, EditorCommands.ParenthesisAlphabeticalList);

        private string theme;

        private static AppViewModel instance;

        private AppViewModel()
        {
            ContentTree = new TreeNode();
            FileSystem = new FileSystem();
            ConverterService = new ConverterService
            {
                Url = Properties.Settings.Default.API
            };
            Workspace = new WorkspaceViewModel();
            Settings = new SettingsViewModel(this);
            Messages = new MessageQueue();
            User = new UserViewModel();
            theme = Properties.Settings.Default.Theme;
        }

        public async Task ChangeWorkspace(string newWorkspace)
        {
            if (string.IsNullOrEmpty(newWorkspace))
                return;

            var cancelArgs = new CancelEventArgs(false);
            await Workspace.OnClosing(cancelArgs);

            // Cancel change
            if (cancelArgs.Cancel)
                return;

            Settings.WorkspacePath = newWorkspace;
            Settings.Save();
            try
            {
                var workspace = await WorkspaceProvider.Create(newWorkspace, FileSystem, ConverterService);
                await Workspace.DataAsync(workspace);
            }
            catch
            {
                Messages.Error(Language.Current.Get("Workspace.Open.Failed"));
            }
            SendPropertyUpdate(nameof(SearchWorkspaceCommand));
        }

        public void SwitchTheme()
        {
            if (theme == "Dark")
            {
                Theme = "Light";
            }
            else
            {
                Theme = "Dark";
            }
            Properties.Settings.Default.Theme = Theme.ToString();
            Properties.Settings.Default.Save();
            //ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.GetAccent("BlueDoc"), ThemeManager.GetAppTheme(theme + "Doc"));
            App.ReloadIcons();
            Workspace.UpdateIcons(Workspace.Children);
        }

        public async Task OnClosing(CancelEventArgs args)
        {
            await Workspace.OnClosing(args);
        }

        private void ShowNode(CommonControlType type)
        {
            if (ContentTree.ContainsType(type))
                return;

            CommonControls.Layout(ContentTree.Grid, type, 0);

            //switch (type)
            //{
            //    case CommonControlType.Explorer:
            //    case CommonControlType.Outline:

            //}

            //ContentTree.Remove(name);
        }
    }
}
