using System.Windows.Input;

namespace Docdown.ViewModel
{
    public class MenuItemAction : ObservableObject
    {
        public string Message { get; set; }
        public string Icon { get; set; }
        public ICommand Command { get; set; }

        public MenuItemAction()
        {

        }

        public MenuItemAction(string message, string icon, ICommand command)
        {
            Message = message;
            Icon = icon;
            Command = command;
        }
    }
}
