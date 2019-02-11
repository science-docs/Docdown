using Docdown.Util;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Docdown.Model
{
    public class Workspace
    {
        public WorkspaceItem Item { get; set; }
        public WorkspaceItem SelectedItem { get; set; }
        public ConverterType FromType => FromSelectedItem();
        public ConverterType ToType { get; set; }
        public bool IgnoreChange
        {
            get => !watcher.EnableRaisingEvents;
            set => watcher.EnableRaisingEvents = !value;
        }

        public event EventHandler WorkspaceChanged;

        private readonly FileSystemWatcher watcher;
        private readonly DispatcherTimer timer;

        public Workspace(string path)
        {
            Item = new WorkspaceItem(path);

            watcher = new FileSystemWatcher(path);
            watcher.Changed += OnWorkspaceChanged;
            watcher.Created += OnWorkspaceChanged;
            watcher.Deleted += OnWorkspaceChanged;
            watcher.Renamed += OnWorkspaceChanged;
            watcher.NotifyFilter = NotifyFilters.LastWrite | 
                                   NotifyFilters.DirectoryName | 
                                   NotifyFilters.FileName;
            watcher.EnableRaisingEvents = true;

            timer = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Normal, OnChangeTick, Application.Current.Dispatcher)
            {
                IsEnabled = false
            };
        }

        private ConverterType FromSelectedItem()
        {
            if (SelectedItem == null)
                return ConverterType.Text;

            switch (SelectedItem.Type)
            {
                case WorkspaceItemType.Markdown:
                    return ConverterType.Markdown;
                case WorkspaceItemType.Latex:
                    return ConverterType.Latex;
                default:
                    return ConverterType.Text;
            }
        }

        public override string ToString()
        {
            return Item?.ToString() ?? base.ToString();
        }

        private void OnWorkspaceChanged(object sender, FileSystemEventArgs e)
        {
            timer.Start();
        }

        private void OnChangeTick(object sender, EventArgs e)
        {
            timer.Stop();
            WorkspaceChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
