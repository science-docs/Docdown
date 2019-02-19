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
        public static Message Empty { get; } = new Message(string.Empty, string.Empty, MessageType.Undefined);

        public string ShortMessage
        {
            get;
            set;
        }

        public string LongMessage
        {
            get;
            set;
        }

        public MessageType Type
        {
            get;
            set;
        }

        public Message()
        {

        }

        public Message(string shortMessage, string longMessage, MessageType type)
        {
            ShortMessage = shortMessage;
            LongMessage = longMessage;
            Type = type;
        }
    }
}
