using Docdown.Model;
using Docdown.Properties;
using Docdown.ViewModel.Commands;
using Docdown.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Docdown.ViewModel
{
    public class UserViewModel : ObservableObject<User>
    {
        public string Name => Data?.Name;
        public string Password => Data?.Password;
        public WebWorkspaceListViewModel[] Workspaces
        {
            get
            {
                if (Data != null)
                {
                    return WebWorkspaceListViewModel.FromPaths(AppViewModel.Instance, Data.Workspaces).ToArray();
                }
                else
                {
                    return new WebWorkspaceListViewModel[0];
                }
            }
        }
        public string Token => Data?.Token;

        public ICommand EnsureLoginCommand => new ActionCommand(() => EnsureLoggedIn());

        private readonly DelegateCommand loginCommand = new OpenWindowCommand<LoginWindow>();
        private readonly LoginViewModel login = new LoginViewModel();

        public UserViewModel() : base(null)
        {
            loginCommand = new OpenWindowCommand<LoginWindow>(login);
            TryLogin();
        }

        private void TryLogin()
        {
            Task.Run(async () =>
            {
                var settings = Settings.Default;
                login.Username = settings.Username;
                login.Password = settings.Password;
                if (!string.IsNullOrEmpty(login.Username) &&
                    !string.IsNullOrEmpty(login.Password))
                {
                    await login.Login();
                    Data = login.User;
                    ForceUpdate();
                }
            });
        }

        public bool EnsureLoggedIn()
        {
            if (Data == null)
            {
                loginCommand.Execute();
                if (login.LoggedIn)
                {
                    Data = login.User;
                    ForceUpdate();
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        public class WebWorkspaceListViewModel : ObservableObject
        {
            public ICommand OpenWorkspaceCommand => new ActionCommand(Open);

            public string Location { get; }
            public AppViewModel App { get; }

            private readonly bool isCreateButton = false;

            public WebWorkspaceListViewModel(AppViewModel app)
            {
                isCreateButton = true;
                Location = "Create...";
                App = app;
            }

            public WebWorkspaceListViewModel(string location, AppViewModel app)
            {
                Location = location;
                App = app;
            }

            public static IEnumerable<WebWorkspaceListViewModel> FromPaths(AppViewModel app, IEnumerable<string> names)
            {
                foreach (var path in names)
                {
                    yield return new WebWorkspaceListViewModel(path, app);
                }
                yield return new WebWorkspaceListViewModel(app);
            }

            public void Open()
            {
                if (isCreateButton)
                {
                    var location = ShowInput("New web workspace", "What is the name of the new web workspace", "New Workspace");
                    if (!string.IsNullOrWhiteSpace(location))
                    {
                        location = location.Trim();
                        App.User.Data.Workspaces.Add(location);
                        App.ChangeWorkspace(Settings.Default.API + "/" + location);
                        App.User.SendPropertyUpdate(nameof(Workspaces));
                    }
                }
                else
                {
                    App.ChangeWorkspace(Settings.Default.API + "/" + Location);
                }
            }

            public override string ToString()
            {
                return Location;
            }
        }
    }
}
