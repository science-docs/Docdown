using System;

namespace Docdown.ViewModel
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
    public class ChangeListenerAttribute : Attribute
    {
        public string[] Properties { get; set; }

        public ChangeListenerAttribute(params string[] properties)
        {
            Properties = properties;
        }
    }
}
