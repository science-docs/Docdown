using System;
using System.Collections.Specialized;
using System.Windows.Documents;

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
            Dispatcher.InvokeAsync(() =>
            {
                var inline = new Run(shortMessage);
                Add(new Message(inline, type));
            });
        }

        public void Add(Inline message, MessageType type)
        {
            if (message.Dispatcher != Dispatcher)
            {
                throw new ArgumentException("Inline was not created on UI Thread");
            }

            Add(new Message(message, type));
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

        public void Success(Inline shortMessage)
        {
            Add(shortMessage, MessageType.Success);
        }

        public void Error(Inline shortMessage)
        {
            Add(shortMessage, MessageType.Error);
        }

        public void Warning(Inline shortMessage)
        {
            Add(shortMessage, MessageType.Warning);
        }

        public void Info(Inline shortMessage)
        {
            Add(shortMessage, MessageType.Undefined);
        }

        public void Working(Inline shortMessage)
        {
            Add(shortMessage, MessageType.Working);
        }

        private void MessagesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SendPropertyUpdate(nameof(NewestMessage));
        }
    }
}