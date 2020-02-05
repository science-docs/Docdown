using Docdown.ViewModel.Commands;
using System;
using System.Windows;
using System.Windows.Input;

namespace Docdown.ViewModel
{
    public class MessageBoxViewModel : ObservableObject
    {
        public string Title { get; set; }

        public string Message { get; set; }

        public bool Save { get; set; }

        public bool SavedValue { get; set; }

        public double Height { get; set; }

        [ChangeListener(nameof(Button))]
        public bool HasOkButton => Index < 2;
        [ChangeListener(nameof(Button))]
        public bool HasNoButton => Index > 2;
        [ChangeListener(nameof(Button))]
        public bool HasYesButton => HasNoButton;
        [ChangeListener(nameof(Button))]
        public bool HasCancelButton => Index % 2 == 1;

        public ICommand OkCommand => new ActionCommand(Set(MessageBoxResult.OK));
        public ICommand CancelCommand => new ActionCommand(Set(MessageBoxResult.Cancel));
        public ICommand YesCommand => new ActionCommand(Set(MessageBoxResult.Yes));
        public ICommand NoCommand => new ActionCommand(Set(MessageBoxResult.No));

        public MessageBoxButton Button
        {
            get => button;
            set => Set(ref button, value);
        }

        public MessageBoxResult Result { get; set; } = MessageBoxResult.None;

        private int Index => (int)button;

        private MessageBoxButton button;

        public MessageBoxViewModel()
        {

        }

        public MessageBoxViewModel(string title, string message, MessageBoxButton button, bool save)
        {
            Title = title;
            Message = message;
            this.button = button;
            Save = save;
            Height = save ? 150 : 130;
        }

        public Action Set(MessageBoxResult result)
        {
            return () => Result = result;
        }
    }
}
