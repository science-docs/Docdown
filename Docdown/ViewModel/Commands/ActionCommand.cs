using System;
using System.Threading.Tasks;

namespace Docdown.ViewModel.Commands
{
    public class ActionCommand : DelegateCommand
    {
        public ActionCommand(Action action) : base(action)
        {
        }

        public ActionCommand(Action action, bool runAsync) : base(runAsync ? (() => Task.Run(action)) : action)
        {

        }
    }
}