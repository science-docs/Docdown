using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Docdown.Util
{
    public static class XExtension
    {
        public static T Attribute<T>(this XElement element, XName name)
        {
            var value = element.Attribute(name)?.Value;
            if (value == null)
            {
                return default;
            }

            var t = typeof(T);
            if (t == typeof(string))
            {
                return (T)(object)value;
            }
            if (t == typeof(int))
            {
                int.TryParse(value, out int i);
                return (T)(object)i;
            }
            if (t == typeof(bool))
            {
                bool.TryParse(value, out bool b);
                return (T)(object)b;
            }
            throw new InvalidCastException();
        }
    }
}
