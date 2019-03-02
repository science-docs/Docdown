using Docdown.ViewModel.Commands;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Docdown.Util
{
    public static class ReflectionUtility
    {
        private static readonly TypeCache<Delegate> staticDelegateCache = new TypeCache<Delegate>();

        public static Delegate CreateDelegate(Type type, object instance)
        {
            if (staticDelegateCache.TryGetValue(type, out Delegate del))
            {
                return del;
            }

            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
            foreach (var method in methods)
            {
                var attr = method.GetCustomAttribute<DelegateAttribute>();
                if (attr != null)
                {
                    Type delType;
                    var types = method.GetParameters().Select(e => e.ParameterType);
                    var isAction = method.ReturnType.Equals(typeof(void));
                    if (isAction)
                    {
                        delType = Expression.GetActionType(types.ToArray());
                    }
                    else
                    {
                        types = types.Concat(method.ReturnType);
                        delType = Expression.GetFuncType(types.ToArray());
                    }

                    if (method.IsStatic)
                    {
                        return staticDelegateCache[type] = Delegate.CreateDelegate(delType, method);
                    }
                    else
                    {
                        return Delegate.CreateDelegate(delType, instance, method.Name);
                    }
                }
            }
            throw new IndexOutOfRangeException();
        }
    }
}
