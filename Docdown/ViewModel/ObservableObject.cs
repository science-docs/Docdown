using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

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
            return Data.GetHashCode();
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
        private static readonly Dictionary<Type, List<PropertyChangedEventHandler>> eventHandlerCache
            = new Dictionary<Type, List<PropertyChangedEventHandler>>();
        
        public string Version { get; } = VersionString;

        protected void Set<T>(ref T field, T value, [CallerMemberName]string property = null)
        {
            field = value;
            SendPropertyUpdate(property);
        }

        protected internal void SendPropertyUpdate([CallerMemberName]string property = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string name)
        {

        }

        protected internal void ForceUpdate()
        {
            var properties = GetType().GetProperties();
            foreach (var prop in properties)
            {
                SendPropertyUpdate(prop.Name);
            }
        }

        protected async Task<MessageDialogResult> ShowMessageAsync(string title, string message, MessageDialogStyle style = MessageDialogStyle.Affirmative, MetroDialogSettings settings = null)
        {
            DialogParticipation.SetRegister(Application.Current.MainWindow, this);
            return await DialogCoordinator.Instance.ShowMessageAsync(this, title, message, style, settings);
        }

        protected async Task<string> ShowInputAsync(string title, string message, MetroDialogSettings settings = null)
        {
            DialogParticipation.SetRegister(Application.Current.MainWindow, this);
            return await DialogCoordinator.Instance.ShowInputAsync(this, title, message, settings);
        }
    
        public ObservableObject()
        {
            PropertyChanged += (_, e) => OnPropertyChanged(e.PropertyName);
            InspectChangeListener();
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
                cache = new List<PropertyChangedEventHandler>();
                foreach (var methodInfo in type
                    .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    RegisterListener(methodInfo, cache);
                }
                foreach (var propertyInfo in type.GetProperties())
                {
                    RegisterListener(propertyInfo, cache);
                }
                eventHandlerCache[type] = cache;
            }
        }

        private void RegisterListener(PropertyInfo property, List<PropertyChangedEventHandler> cache)
        {
            var listenerAttribute = property.GetCustomAttribute<ChangeListenerAttribute>();

            if (listenerAttribute != null)
            {
                string emitterProperty = listenerAttribute.Property;
                string listenerProperty = property.Name;
                PropertyChangedEventHandler handler = AttributePropertyChanged;
                cache.Add(handler);
                PropertyChanged += handler;

                void AttributePropertyChanged(object sender, PropertyChangedEventArgs e)
                {
                    if (e.PropertyName == emitterProperty)
                    {
                        PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(listenerProperty));
                    }
                }
            }
        }

        private void RegisterListener(MethodInfo method, List<PropertyChangedEventHandler> cache)
        {
            var listenerAttribute = method.GetCustomAttribute<ChangeListenerAttribute>();

            if (listenerAttribute != null)
            {
                var param = method.GetParameters();

                if (param.Length > 0)
                    throw new IndexOutOfRangeException("Only methods without parameters are supported");

                string emitterProperty = listenerAttribute.Property;
                PropertyChangedEventHandler handler = CallListenerMethod;
                cache.Add(handler);
                PropertyChanged += handler;

                void CallListenerMethod(object sender, PropertyChangedEventArgs e)
                {
                    if (e.PropertyName == emitterProperty)
                    {
                        method.Invoke(this, new object[0]);
                    }
                }
            }
        }
    }
}