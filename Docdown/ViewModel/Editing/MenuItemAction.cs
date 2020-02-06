using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Docdown.ViewModel.Editing
{
    public class MenuItemAction : ObservableObject
    {
        public string Message { get; set; }
        public string Icon { get; set; }
        public ICommand Command { get; set; }
        public ObservableCollection<MenuItemAction> Children { get; }

        public MenuItemAction()
        {
            Children = new ObservableCollection<MenuItemAction>();
        }

        public MenuItemAction(string message, string icon, ICommand command) : this(message, icon, command, null)
        {
            Message = message;
            Icon = icon;
            Command = command;
        }

        public MenuItemAction(string message, string icon, ICommand command, IEnumerable<MenuItemAction> children) : this()
        {
            Message = message;
            Icon = icon;
            Command = command;

            if (children != null)
            {
                Children = new ObservableCollection<MenuItemAction>(children);
            }
        }
    }
}
