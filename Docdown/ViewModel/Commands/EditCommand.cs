using Docdown.Editor;
using Docdown.Editor.Commands;
using System;
using System.Windows.Input;

namespace Docdown.ViewModel.Commands
{
    public class EditCommand : DelegateCommand
    {
        public EditCommand(AppViewModel item, EditorCommand command) : base(item, command)
        {

        }

        [Delegate]
        private static void Edit(AppViewModel item, EditorCommand command)
        {
            var view = item?.Workspace?.SelectedItem?.View;
            if (view is IEditor editor)
            {
                command.Execute(editor.Editor);
                Keyboard.Focus(editor.Editor);
            }
        }
    }
}
