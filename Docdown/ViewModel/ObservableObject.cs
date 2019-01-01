using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

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
        
        public string Version { get; } = VersionString;

        protected void Set<T>(ref T field, T value, [CallerMemberName]string property = null)
        {
            field = value;
            SendPropertyUpdate(property);
        }

        protected void SendPropertyUpdate([CallerMemberName]string property = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string name)
        {

        }
    
        public ObservableObject()
        {
            PropertyChanged += (_, e) => OnPropertyChanged(e.PropertyName);
            InspectChangeListener();
        }

        private void InspectChangeListener()
        {
            foreach (var methodInfo in GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                RegisterListener(methodInfo);
            }
            foreach (var propertyInfo in GetType().GetProperties())
            {
                RegisterListener(propertyInfo);
            }
        }

        private void RegisterListener(PropertyInfo property)
        {
            var listenerAttribute = property.GetCustomAttribute<ChangeListenerAttribute>();

            if (listenerAttribute != null)
            {
                string emitterProperty = listenerAttribute.Property;
                string listenerProperty = property.Name;
                PropertyChanged += AttributePropertyChanged;

                void AttributePropertyChanged(object sender, PropertyChangedEventArgs e)
                {
                    if (e.PropertyName == emitterProperty)
                    {
                        PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(listenerProperty));
                    }
                }
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
                PropertyChanged += CallListenerMethod;

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