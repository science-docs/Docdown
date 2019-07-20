using System;
using System.Threading.Tasks;

namespace Docdown.ViewModel.Commands
{
    public class ActionCommand : DelegateCommand
    {
        public ActionCommand(Action action) : base(action)
        {
        }

        public ActionCommand(Action action, bool runAsync) : this(runAsync ? () => Task.Run(action) : action)
        {
        }

        public ActionCommand(Func<Task> action) : this(() => { Task.Run(action); })
        {

        }
    }
}