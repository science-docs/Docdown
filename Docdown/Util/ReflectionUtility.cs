using Docdown.ViewModel.Commands;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows.Threading;

namespace Docdown.Util
{
    public static class ReflectionUtility
    {
        private static readonly TypeCache<DelegateSTA> staticDelegateCache = new TypeCache<DelegateSTA>();

        public static void EnsureMainThread()
        {
            if (System.Windows.Application.Current.Dispatcher != Dispatcher.CurrentDispatcher)
            {
                throw new InvalidOperationException("Can only be called from the main thread. Try using a dispatcher");
            }
        }

        public static Delegate CreateDelegate(Type type, object instance, out bool sta)
        {
            if (staticDelegateCache.TryGetValue(type, out DelegateSTA del))
            {
                sta = del.STA;
                return del.Delegate;
            }

            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
            foreach (var method in methods)
            {
                var attr = method.GetCustomAttribute<DelegateAttribute>();
                if (attr != null)
                {
                    sta = method.GetCustomAttribute<STAThreadAttribute>() != null;

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
                        var created = Delegate.CreateDelegate(delType, method);
                        staticDelegateCache[type] = new DelegateSTA(created, sta);
                        return created;
                    }
                    else
                    {
                        return Delegate.CreateDelegate(delType, instance, method.Name);
                    }
                }
            }
            throw new IndexOutOfRangeException();
        }

        private struct DelegateSTA
        {
            public Delegate Delegate;
            public bool STA;

            public DelegateSTA(Delegate del, bool sta)
            {
                Delegate = del;
                STA = sta;
            }
        }
    }
}
