using System.Collections.Specialized;

namespace Docdown.ViewModel
{
    public class MessageQueue : ObservableList<Message>
    {
        public Message NewestMessage => Count > 0 ? this[Count - 1] : null;

        public MessageQueue()
        {
            CollectionChanged += MessagesChanged;
        }

        public void Add(string shortMessage, string longMessage, MessageType type)
        {
            Add(new Message(shortMessage, longMessage, type));
        }

        public void Success(string shortMessage, string longMessage)
        {
            Add(shortMessage, longMessage, MessageType.Success);
        }

        public void Error(string shortMessage, string longMessage)
        {
            Add(shortMessage, longMessage, MessageType.Error);
        }

        public void Warning(string shortMessage, string longMessage)
        {
            Add(shortMessage, longMessage, MessageType.Warning);
        }

        public void Info(string shortMessage, string longMessage)
        {
            Add(shortMessage, longMessage, MessageType.Undefined);
        }

        public void Working(string shortMessage, string longMessage)
        {
            Add(shortMessage, longMessage, MessageType.Working);
        }

        private void MessagesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SendPropertyUpdate(nameof(NewestMessage));
        }
    }
}