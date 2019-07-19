using Docdown.Editor;
using Docdown.Editor.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Docdown.ViewModel.Commands
{
    public class EditCommand : DelegateCommand
    {
        public EditCommand(WorkspaceItemViewModel item, EditorCommand command) : base(item, command)
        {

        }

        [Delegate]
        private static void Edit(WorkspaceItemViewModel item, EditorCommand command)
        {
            var view = item.View;
            if (view is IEditor editor)
            {
                command.Execute(editor.Editor);
            }
        }
    }
}
