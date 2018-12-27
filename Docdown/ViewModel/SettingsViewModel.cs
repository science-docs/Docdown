using Docdown.Properties;
using Docdown.ViewModel.Commands;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;

namespace Docdown.ViewModel
{
    public class SettingsViewModel : ObservableObject
    {
        private readonly Settings settings;

        public ICommand SaveCommand => new ActionCommand(Save);
        public ICommand RestoreCommand => new ActionCommand(Restore);

        public const int MaxLastWorkspaces = 5;

        private readonly Action<string> changeWorkspace;

        public SettingsViewModel(Action<string> changeWorkspace)
        {
            this.changeWorkspace = changeWorkspace;
            settings = Settings.Default;
            if (settings.LastWorkspaces == null)
                settings.LastWorkspaces = new StringCollection();

            SetLastWorkspaces(settings.LastWorkspaces.OfType<string>());
        }
        
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

        public string Template
        {
            get => settings.Template;
            set
            {
                settings.Template = value;
                SendPropertyUpdate();
            }
        }

        [ChangeListener(nameof(WorkspacePath))]
        public IEnumerable<LastWorkspaceViewModel> LastWorkspaces { get; private set; }

        public void Restore()
        {
            settings.Reset();
        }

        public void Save()
        {
            settings.Save();
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
            LastWorkspaceViewModel.Count = 1;
            LastWorkspaces = values.Select(e => new LastWorkspaceViewModel(e, changeWorkspace)).ToArray();
        }

        public class LastWorkspaceViewModel
        {
            public ICommand OpenWorkspaceCommand => new ActionCommand(() => changeWorkspace(Location));

            public string Location { get; set; }
            public string Text => $"{Index}. {Location}";
            public int Index { get; set; }

            public static int Count { get; set; }

            private readonly Action<string> changeWorkspace;

            public LastWorkspaceViewModel(string location, Action<string> changeWorkspaceCallback)
            {
                Location = location;
                Index = Count++;
                changeWorkspace = changeWorkspaceCallback;
            }

            public override string ToString()
            {
                return Text;
            }
        }
    }
}