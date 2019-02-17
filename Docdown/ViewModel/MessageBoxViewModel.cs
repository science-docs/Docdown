using Docdown.ViewModel.Commands;
using System.Windows;
using System.Windows.Input;

namespace Docdown.ViewModel
{
    public class MessageBoxViewModel : ObservableObject
    {
        public string Title { get; set; }

        public string Message { get; set; }

        [ChangeListener(nameof(Button))]
        public bool HasOkButton => Index < 2;
        [ChangeListener(nameof(Button))]
        public bool HasNoButton => Index > 2;
        [ChangeListener(nameof(Button))]
        public bool HasYesButton => HasNoButton;
        [ChangeListener(nameof(Button))]
        public bool HasCancelButton => Index % 2 == 1;

        public ICommand OkCommand => new ActionCommand(() => Result = MessageBoxResult.OK);
        public ICommand CancelCommand => new ActionCommand(() => Result = MessageBoxResult.Cancel);
        public ICommand YesCommand => new ActionCommand(() => Result = MessageBoxResult.Yes);
        public ICommand NoCommand => new ActionCommand(() => Result = MessageBoxResult.No);

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

        public MessageBoxViewModel(string title, string message, MessageBoxButton button)
        {
            Title = title;
            Message = message;
            this.button = button;
        }
    }
}
