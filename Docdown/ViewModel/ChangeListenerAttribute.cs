using System;

namespace Docdown.ViewModel
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
    public class ChangeListenerAttribute : Attribute
    {
        public string Property { get; set; }

        public ChangeListenerAttribute(string property)
        {
            Property = property;
        }
    }
}
