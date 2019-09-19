using System.Windows.Documents;

namespace Docdown.ViewModel
{
    public enum MessageType : byte
    {
        Undefined = 0,
        Success = 1,
        Warning = 2,
        Error = 3,
        Working = 4
    }

    public class Message : ObservableObject
    {
        public static Message Empty { get; } = new Message(string.Empty, MessageType.Undefined);

        public Inline Content
        {
            get => content;
            private set => Set(ref content, value);
        }

        public MessageType Type { get; }

        private Inline content;

        public Message(string shortMessage, MessageType type)
        {
            SetContentAsync(shortMessage);
            Type = type;
        }

        public Message(Inline message, MessageType type)
        {
            Content = message;
            Type = type;
        }

        private void SetContentAsync(string text)
        {
            Dispatcher.InvokeAsync(() =>
            {
                Content = new Run(text);
            });
        }
    }
}
