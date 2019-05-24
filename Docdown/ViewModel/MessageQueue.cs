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

        public void Add(string shortMessage, MessageType type)
        {
            Add(new Message(shortMessage, type));
        }

        public void Success(string shortMessage)
        {
            Add(shortMessage, MessageType.Success);
        }

        public void Error(string shortMessage)
        {
            Add(shortMessage, MessageType.Error);
        }

        public void Warning(string shortMessage)
        {
            Add(shortMessage, MessageType.Warning);
        }

        public void Info(string shortMessage)
        {
            Add(shortMessage, MessageType.Undefined);
        }

        public void Working(string shortMessage)
        {
            Add(shortMessage, MessageType.Working);
        }

        private void MessagesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SendPropertyUpdate(nameof(NewestMessage));
        }
    }
}