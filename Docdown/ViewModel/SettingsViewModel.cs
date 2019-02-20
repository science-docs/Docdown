using Docdown.Model;
using Docdown.Properties;
using Docdown.Util;
using Docdown.ViewModel.Commands;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Docdown.ViewModel
{
    public class SettingsViewModel : ObservableObject
    {
        public ICommand SaveCommand => new ActionCommand(Save);
        public ICommand RestoreCommand => new ActionCommand(Restore);
        public ICommand TestConnectionCommand => new ActionCommand(TestConnection, true);
        public ICommand UploadTemplateCommand => new ActionCommand(UploadTemplate);

        public string WorkspacePath
        {
            get => settings.WorkspacePath;
            set
            {
                PutWorkspace(value);
                SendPropertyUpdate();
            }
        }

        public string Api
        {
            get => settings.API;
            set
            {
                settings.API = value;
                SendPropertyUpdate();
            }
        }

        [ChangeListener(nameof(Templates))]
        public Template SelectedTemplate
        {
            get => Templates.SingleOrDefault(e => e.Name == selectedTemplateName) ?? Template.Empty;
            set
            {
                if (value == null)
                {
                    value = Template.Empty;
                }
                selectedTemplateName = value.Name;
                settings.Template = value.Name;
                SendPropertyUpdate();
            }
        }

        public Template[] Templates
        {
            get => templates;
            set => Set(ref templates, value);
        }

        public bool UseOfflineCompiler
        {
            get => settings.UseOfflineCompiler;
            set
            {
                settings.UseOfflineCompiler = value;
                SendPropertyUpdate();
            }
        }

        public ConnectionStatus ConnectionStatus
        {
            get => connectionStatus;
            set => Set(ref connectionStatus, value);
        }

        [ChangeListener(nameof(ConnectionStatus))]
        public bool IsConnected => ConnectionStatus == ConnectionStatus.Connected;

        [ChangeListener(nameof(WorkspacePath))]
        public IEnumerable<LastWorkspaceViewModel> LastWorkspaces { get; private set; }

        public const int MaxLastWorkspaces = 5;

        private readonly Settings settings;
        private readonly WorkspaceViewModel workspace;
        private Template[] templates;
        private string selectedTemplateName;
        private ConnectionStatus connectionStatus;

        public SettingsViewModel(WorkspaceViewModel workspace)
        {
            this.workspace = workspace;
            settings = Settings.Default;
            if (settings.LastWorkspaces == null)
                settings.LastWorkspaces = new StringCollection();

            selectedTemplateName = settings.Template;
            SetLastWorkspaces(settings.LastWorkspaces.OfType<string>());
        }

        public void Restore()
        {
            settings.Reset();
            selectedTemplateName = string.Empty;
            connectionStatus = ConnectionStatus.Undefined;
            ForceUpdate();
        }

        public void Save()
        {
            settings.Save();
        }

        public void TestConnection()
        {
            if (ConnectionStatus != ConnectionStatus.Connecting)
            {
                ConnectionStatus = ConnectionStatus.Connecting;
                ConnectionStatus = WebUtility.Ping();
                if (IsConnected)
                {
                    LoadTemplates();
                    workspace.Messages.Success("Connected to server", "Connected to server");
                }
                else
                {
                    Templates = new[] { Template.Empty };
                    workspace.Messages.Error("Could not connect to server", "Could not connect to server");
                }
            }
        }

        public void LoadTemplates()
        {
            string templatesUrl = WebUtility.BuildTemplatesUrl();

            string text;
            try
            {
                using (var res = WebUtility.SimpleGetRequest(templatesUrl))
                {
                    using (var rs = res.GetResponseStream())
                    {
                        using (var sr = new StreamReader(rs))
                        {
                            text = sr.ReadToEnd();
                        }
                    }
                }
            }
            catch
            {
                text = "[]";
            }

            try
            {
                Templates = Template.FromJson(text).OrderBy(e => e.Name).ToArray();
            }
            catch
            {
                SelectedTemplate = null;
                Templates = new Template[] { Template.Empty };
            }
        }

        public void UploadTemplate(string path)
        {
            var nameParam = MultipartFormParameter.CreateField("name", Path.GetFileName(path));
            var parameter = MultipartFormParameter.FromFolder(path).Concat(new[] { nameParam });
            try
            {
                WebUtility.MultipartFormDataPost(WebUtility.BuildTemplatesUrl(), parameter).GetResponse().Dispose();
                LoadTemplates();
            }
            catch
            {

            }
        }

        private void UploadTemplate()
        {
            if (!IsConnected)
                return;

            string templateFolder = new SearchFolderCommand(null, "Select template").ExecuteWithResult();
            if (!string.IsNullOrEmpty(templateFolder))
            {
                Task.Run(() =>
                {
                    UploadTemplate(templateFolder);
                });
            }
        }

        [ChangeListener(nameof(UseOfflineCompiler))]
        private void UseOfflineCompilerChanged()
        {
            if (UseOfflineCompiler)
            {
                ConnectionStatus = ConnectionStatus.Undefined;
            }
            else
            {
                Task.Run((Action)TestConnection);
            }
        }

        [ChangeListener(nameof(Api))]
        private void ApiChanged()
        {
            ConnectionStatus = ConnectionStatus.Undefined;
        }

        private void PutWorkspace(string newWorkspace)
        {
            var old = WorkspacePath;
            if (!string.IsNullOrEmpty(old))
            {
                var items = settings.LastWorkspaces.OfType<string>().Take(MaxLastWorkspaces - 1);
                var list = new LinkedList<string>(items);
                list.AddFirst(old);
                var distinct = list.Distinct();
                SetLastWorkspaces(distinct);
                settings.LastWorkspaces.Clear();
                settings.LastWorkspaces.AddRange(distinct.ToArray());
            }
            settings.WorkspacePath = newWorkspace;
        }

        private void SetLastWorkspaces(IEnumerable<string> values)
        {
            LastWorkspaces = LastWorkspaceViewModel.FromPaths(workspace, values).ToArray();
        }

        public class LastWorkspaceViewModel
        {
            public ICommand OpenWorkspaceCommand => new ActionCommand(() => Workspace.ChangeWorkspace(Location));

            public string Location { get; }
            public string Text => $"{Index} {Location}";
            public int Index { get; }
            public WorkspaceViewModel Workspace { get; }

            public LastWorkspaceViewModel(string location, WorkspaceViewModel workspace, int index)
            {
                Location = location;
                Index = index;
                Workspace = workspace;
            }

            public static IEnumerable<LastWorkspaceViewModel> FromPaths(WorkspaceViewModel workspace, IEnumerable<string> paths)
            {
                int count = 1;
                foreach (var path in paths)
                {
                    yield return new LastWorkspaceViewModel(path, workspace, count++);
                }
            }

            public override string ToString()
            {
                return Text;
            }
        }
    }
}