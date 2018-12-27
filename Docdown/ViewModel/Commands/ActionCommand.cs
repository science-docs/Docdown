using System;

namespace Docdown.ViewModel.Commands
{
    public class ActionCommand : DelegateCommand
    {
        public ActionCommand(Action action) : base(action)
        {
        }
    }
}