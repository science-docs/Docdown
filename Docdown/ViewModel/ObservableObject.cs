using Docdown.Util;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Docdown.ViewModel
{
    public abstract class ObservableObject<T> : ObservableObject
    {
        public T Data
        {
            get => data;
            set => Set(ref data, value);
        }

        private T data;

        public ObservableObject(T data) : base()
        {
            this.data = data;
        }

        public override bool Equals(object obj)
        {
            if (obj is ObservableObject<T> oo)
            {
                return data.Equals(oo.data);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Data?.GetHashCode() ?? base.GetHashCode();
        }

        public override string ToString()
        {
            return Data?.ToString() ?? base.ToString();
        }
    }

    public abstract class ObservableObject : INotifyPropertyChanged
    {
        private static readonly string VersionString
            = typeof(ObservableObject).Assembly.GetName().Version.ToString();
        private static readonly ListTypeCache<PropertyChangedEventHandler> eventHandlerCache
            = new ListTypeCache<PropertyChangedEventHandler>();

        public string Version => VersionString;
        public Dispatcher Dispatcher => Application.Current.Dispatcher;
        public MessageQueue Messages => AppViewModel.Instance.Messages;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableObject()
        {
            PropertyChanged += (_, e) => OnPropertyChanged(e.PropertyName);
            InspectChangeListener();
        }

        protected virtual void OnPropertyChanged(string name)
        {

        }

        protected void Set<T>(ref T field, T value, [CallerMemberName]string property = null)
        {
            field = value;
            SendPropertyUpdate(property);
        }

        protected internal void SendPropertyUpdate([CallerMemberName]string property = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        protected internal void ForceUpdate()
        {
            var properties = GetType().GetProperties();
            foreach (var prop in properties)
            {
                SendPropertyUpdate(prop.Name);
            }
        }

        protected MessageBoxResult ShowMessage(string title, string message, MessageBoxButton button)
        {
            return Util.MessageBox.Show(title, message, button);
        }

        protected async Task<MessageBoxResult> ShowMessageAsync(string title, string message, MessageBoxButton button)
        {
            return await Dispatcher.InvokeAsync(() => Util.MessageBox.Show(title, message, button));
        }

        protected string ShowInput(string title, string message, string pretext)
        {
            return InputBox.Show(title, message, pretext);
        }

        protected async Task<string> ShowInputAsync(string title, string message, string pretext)
        {
            return await Dispatcher.InvokeAsync(() => InputBox.Show(title, message, pretext));
        }

        private void InspectChangeListener()
        {
            var type = GetType();
            if (eventHandlerCache.TryGetValue(type, out var cache))
            {
                foreach (var handler in cache)
                {
                    PropertyChanged += handler;
                }
            }
            else
            {
                foreach (var methodInfo in type
                    .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    RegisterListener(methodInfo);
                }
                foreach (var propertyInfo in type.GetProperties())
                {
                    RegisterListener(propertyInfo);
                }
            }
        }

        private void RegisterListener(PropertyInfo property)
        {
            var listenerAttribute = property.GetCustomAttribute<ChangeListenerAttribute>();

            if (listenerAttribute != null)
            {
                string emitterProperty = listenerAttribute.Property;
                PropertyChangedEventHandler handler = CreatePropertyListenerHandler(emitterProperty, property);
                eventHandlerCache.Add(GetType(), handler);
                PropertyChanged += handler;
            }
        }

        private void RegisterListener(MethodInfo method)
        {
            var listenerAttribute = method.GetCustomAttribute<ChangeListenerAttribute>();

            if (listenerAttribute != null)
            {
                var param = method.GetParameters();

                if (param.Length > 0)
                    throw new IndexOutOfRangeException("Only methods without parameters are supported");

                string emitterProperty = listenerAttribute.Property;
                PropertyChangedEventHandler handler = CreateCallListenerHandler(emitterProperty, method);
                eventHandlerCache.Add(GetType(), handler);
                PropertyChanged += handler;
            }
        }

        private static PropertyChangedEventHandler CreatePropertyListenerHandler(string emitter, PropertyInfo listener)
        {
            string listenerName = listener.Name;
            return AttributePropertyChanged;

            void AttributePropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == emitter &&
                    sender is ObservableObject observableObject)
                {
                    observableObject.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(listenerName));
                }
            }
        }

        private static PropertyChangedEventHandler CreateCallListenerHandler(string emitter, MethodInfo listener)
        {
            return CallListenerMethod;

            void CallListenerMethod(object sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == emitter)
                {
                    listener.Invoke(sender, new object[0]);
                }
            }
        }
    }
}