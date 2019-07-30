using Docdown.Model;
using Docdown.ViewModel.Commands;
using System;
using System.Linq;
using WebUtility = Docdown.Util.WebUtility;
using System.Security;
using System.Windows.Input;
using Docdown.Util;
using System.Net;
using Newtonsoft.Json.Linq;
using Docdown.Properties;
using System.Threading.Tasks;

namespace Docdown.ViewModel
{
    public class LoginViewModel : ObservableObject
    {
        public string Username { get; set; }
        public string Password
        {
            get => credentials.Password;
            set => credentials.Password = value;
        }
        public SecureString SecurePassword
        {
            get => credentials.SecurePassword;
            set => credentials.SecurePassword = value;
        }

        public string Error
        {
            get => error;
            set => Set(ref error, value);
        }

        public event EventHandler CloseRequested;

        public User User { get; private set; }
        public bool LoggedIn => User != null;

        public ICommand RegisterCommand => new ActionCommand(Register);
        public ICommand LoginCommand => new ActionCommand(Login);

        private readonly NetworkCredential credentials = new NetworkCredential(string.Empty, string.Empty);
        private string error;

        public async Task Register()
        {
            try
            {
                var res = await WebUtility.PostRequest(WebUtility.BuildRegisterUrl(),
                MultipartFormParameter.CreateField("username", Username),
                MultipartFormParameter.CreateField("password", Password));

                await res.Content.ReadAsStringAsync();

                await Login();
            }
            catch (Exception e)
            {
                Error = ErrorUtility.GetErrorMessage(e);
            }
        }

        public async Task Login()
        {
            try
            {
                var settings = Settings.Default;
                settings.Username = Username;
                settings.Password = Password;
                settings.Save();

                var res = await WebUtility.PostRequest(WebUtility.BuildLoginUrl(),
                MultipartFormParameter.CreateField("username", Username),
                MultipartFormParameter.CreateField("password", Password));

                var tokenJson = await res.Content.ReadAsStringAsync();
                var json = JObject.Parse(tokenJson);

                var token = json.SelectToken("token").Value<string>();
                var workspaces = json.SelectToken("workspaces").Values<string>().OrderBy(e => e);

                User = new User
                {
                    Name = Username,
                    Password = Password,
                    Token = token
                };
                User.Workspaces.AddRange(workspaces);
                await Dispatcher.InvokeAsync(() => CloseRequested?.Invoke(this, EventArgs.Empty));
            }
            catch (Exception e)
            {
                Error = ErrorUtility.GetErrorMessage(e);
            }

        }
    }
}
