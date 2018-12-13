using System;
using System.Windows.Input;

namespace Docdown.ViewModel
{
    public class ActionCommand : ICommand
    {
        private readonly Action action;

        public ActionCommand(Action action)
        {
            this.action = action;
        }

        public void Execute(object parameter)
        {
            action();
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        #pragma warning disable CS0067 // Unused Event
        public event EventHandler CanExecuteChanged;
        #pragma warning restore CS0067
    }
}