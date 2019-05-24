using Docdown.Model;
using Docdown.Properties;
using Docdown.Util;
using Docdown.ViewModel.Commands;
using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json.Linq;
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
        public ICommand SearchTemplateCommand => new SearchFileCommand(null, "Search template", template => SelectedLocaleTemplate = template, TemplateFileFilter);
        public ICommand SearchCitationStyleCommand => new SearchFileCommand(null, "Search citation style", csl => SelectedLocaleCsl = csl, CslFileFilter);

        private static readonly CommonFileDialogFilter CslFileFilter = new CommonFileDialogFilter("Citation Style File", ".csl");
        private static readonly CommonFileDialogFilter TemplateFileFilter = new CommonFileDialogFilter("LaTeX Template File", ".latex");

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

        public string SelectedCsl
        {
            get => settings.Csl;
            set
            {
                settings.Csl = value;
                SendPropertyUpdate();
            }
        }

        public string SelectedLocaleTemplate
        {
            get => settings.LocaleTemplate;
            set
            {
                settings.LocaleTemplate = value;
                SendPropertyUpdate();
            }
        }

        public string SelectedLocaleCsl
        {
            get => settings.LocaleCsl;
            set
            {
                settings.LocaleCsl = value;
                SendPropertyUpdate();
            }
        }

        [ChangeListener(nameof(Templates))]
        public Template SelectedTemplate
        {
            get => Templates.SingleOrDefault(e => e.Name == selectedTemplateName) ?? Template.Empty;
            set
            {
                if (value is null)
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

        public string[] Csls
        {
            get => csls;
            set => Set(ref csls, value);
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

        public bool CompileOnlySelected
        {
            get => settings.CompileOnlySelected;
            set
            {
                settings.CompileOnlySelected = value;
                SendPropertyUpdate();
            }
        }

        public ConnectionStatus ConnectionStatus
        {
            get => connectionStatus;
            set => Set(ref connectionStatus, value);
        }

        public bool CanCompileOffline { get; }

        [ChangeListener(nameof(ConnectionStatus))]
        public bool IsConnected => ConnectionStatus == ConnectionStatus.Connected;

        [ChangeListener(nameof(WorkspacePath))]
        public IEnumerable<LastWorkspaceViewModel> LastWorkspaces { get; private set; }

        public const int MaxLastWorkspaces = 5;

        private readonly Settings settings;
        private readonly WorkspaceViewModel workspace;
        private Template[] templates;
        private string[] csls;
        private string selectedTemplateName;
        private ConnectionStatus connectionStatus;

        public SettingsViewModel(WorkspaceViewModel workspace)
        {
            this.workspace = workspace;
            settings = Settings.Default;
            if (settings.LastWorkspaces is null)
                settings.LastWorkspaces = new StringCollection();

            selectedTemplateName = settings.Template;
            SetLastWorkspaces(settings.LastWorkspaces.OfType<string>());

            CanCompileOffline = PandocUtility.Exists();
            if (!CanCompileOffline)
            {
                UseOfflineCompiler = false;
            }
        }

        public void Restore()
        {
            settings.Reset();
            selectedTemplateName = string.Empty;
            SelectedCsl = string.Empty;
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
                    LoadCsls();
                    workspace.Messages.Success("Connected to server", "Connected to server");
                }
                else
                {
                    Templates = new[] { Template.Empty };
                    Csls = new[] { string.Empty };
                    workspace.Messages.Error("Could not connect to server", "Could not connect to server");
                }
            }
        }

        public void LoadCsls()
        {
            string cslUri = WebUtility.BuildCslUrl();

            string text;
            try
            {
                text = WebUtility.SimpleTextRequest(cslUri);
            }
            catch
            {
                text = "[]";
            }

            try
            {
                Csls = JArray.Parse(text)
                    .Select(e => e.Value<string>())
                    .Concat(string.Empty)
                    .OrderBy(e => e)
                    .ToArray();
                SelectedCsl = settings.Csl;
            }
            catch
            {
                SelectedCsl = string.Empty;
                Csls = new[] { string.Empty };
            }
        }

        public void LoadTemplates()
        {
            string templatesUrl = WebUtility.BuildTemplatesUrl();

            string text;
            try
            {
                text = WebUtility.SimpleTextRequest(templatesUrl);
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
                Templates = new[] { Template.Empty };
            }
        }

        public void UploadTemplate(string path)
        {
            var nameParam = MultipartFormParameter.CreateField("name", Path.GetFileName(path));
            var parameter = MultipartFormParameter.FromFolder(path).Concat(nameParam);
            try
            {
                WebUtility.MultipartFormDataPost(WebUtility.BuildTemplatesUrl(), parameter).GetResponse().Dispose();
                LoadTemplates();
                workspace.Messages.Success("Successfully uploaded template", string.Empty);
            }
            catch
            {
                workspace.Messages.Error("Could not upload template", string.Empty);
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