using System;
using System.Collections.Generic;

namespace Docdown.Util
{
    public class TypeCache<T> : Dictionary<Type, T>
    {

    }

    public class ListTypeCache<T> : Dictionary<Type, List<T>>
    {
        public void Add(Type key, T value)
        {
            if (TryGetValue(key, out var list))
            {
                list.Add(value);
            }
            else
            {
                list = new List<T>
                {
                    value
                };
                this[key] = list;
            }
        }
    }
}
