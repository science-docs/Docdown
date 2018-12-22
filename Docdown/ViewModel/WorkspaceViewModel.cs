using Docdown.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Docdown.ViewModel
{
    public class WorkspaceViewModel : ObservableObject<Workspace>
    {
        public WorkspaceItemViewModel Item
        {
            get => new WorkspaceItemViewModel(Data.Item);
        }
        public WorkspaceItemViewModel SelectedItem
        {
            get => new WorkspaceItemViewModel(Data.SelectedItem);
            set
            {
                Data.SelectedItem = value.Data;
                SendPropertyUpdate();
                SendPropertyUpdate(nameof(SelectedItemName));
                if (Data.SelectedItem.IsPlainText())
                {
                    CanConvert = true;
                    SendPropertyUpdate(nameof(SelectedItemText));
                }
                else
                {
                    CanConvert = false;
                }
            }
        }

        public string SelectedItemName
        {
            get
            {
                var item = Data.SelectedItem;
                if (item != null)
                {
                    return item.FileSystemInfo.Name;
                }
                return "";
            }
        }

        public string SelectedItemText
        {
            get
            {
                var selectedItem = Data.SelectedItem;
                if (selectedItem != null)
                {
                    string fullName = selectedItem.FileSystemInfo.FullName;
                    if (!textCache.TryGetValue(fullName, out string cachedText)
                        && selectedItem.IsPlainText())
                    {
                        cachedText = File.ReadAllText(fullName);
                        textCache[fullName] = cachedText;
                    }
                    return cachedText;
                }
                return string.Empty;
            }
            set
            {
                var selectedItem = Data.SelectedItem;
                if (selectedItem != null)
                {
                    string fullName = selectedItem.FileSystemInfo.FullName;
                    textCache[fullName] = value;
                }
            }
        }

        public bool CanConvert
        {
            get => canConvert && !isConverting;
            set => Set(ref canConvert, value);
        }

        public bool IsConverting
        {
            get => isConverting;
            set
            {
                Set(ref isConverting, value);
                SendPropertyUpdate(nameof(CanConvert));
            }
        }

        public ICommand SaveSelectedItemCommand => new ActionCommand(SaveSelectedItem);
        public ICommand SaveAllItemsCommand => new ActionCommand(SaveAllItems);
        public ICommand ConvertCommand => new ActionCommand(Convert);

        public event EventHandler SelectedItemTextChanged;

        private bool canConvert;
        private bool isConverting;
        private Dictionary<string, string> textCache = new Dictionary<string, string>();

        public IEnumerable<WorkspaceItemViewModel> Children
            => Data.Item.Children.Select(e => new WorkspaceItemViewModel(e));

        public WorkspaceViewModel(Workspace workspace) : base(workspace)
        {

        }

        protected override void OnPropertyChanged(string name)
        {
            if (name == nameof(SelectedItemText))
                SelectedItemTextChanged?.Invoke(this, EventArgs.Empty);
        }

        private void SaveSelectedItem()
        {
            var selectedItem = Data.SelectedItem;
            if (selectedItem != null)
            {
                string fullName = selectedItem.FileSystemInfo.FullName;
                File.WriteAllText(fullName, textCache[fullName]);
            }
        }

        private void SaveAllItems()
        {
            foreach (var pair in textCache)
            {
                File.WriteAllText(pair.Key, pair.Value);
            }
        }

        private void Convert()
        {
            IsConverting = true;
            Task.Run(() =>
            {
                try
                {
                    Data.Convert();
                }
                catch
                {

                }
                IsConverting = false;
            });
        }
    }
}
