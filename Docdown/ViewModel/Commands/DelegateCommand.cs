using Docdown.Util;
using System;
using System.Linq;
using System.Windows.Input;

namespace Docdown.ViewModel.Commands
{
    [AttributeUsage(AttributeTargets.Method)]
    public class DelegateAttribute : Attribute
    {

    }

    public class DelegateCommand<T> : DelegateCommand
    {
        public new T Result
        {
            get
            {
                if (base.Result is T value)
                {
                    return value;
                }
                return default;
            }
        }

        public DelegateCommand(params object[] parameters) : base(typeof(T), parameters)
        {

        }

        public DelegateCommand(Delegate del, params object[] parameters) : base(del, parameters)
        {
            CheckReturnType(del);
        }

        public T ExecuteWithResult()
        {
            Execute();
            return Result;
        }

        private void CheckReturnType(Delegate del)
        {
            var returnType = del.Method.ReturnType;
            if (!typeof(T).IsAssignableFrom(returnType))
            {
                throw new InvalidCastException();
            }
        }
    }

    public class DelegateCommand : ICommand
    {
        public object Result { get; private set; }

        private readonly Delegate del;
        private readonly object[] parameters;
        private bool usesAdditionalParameter = false;

#pragma warning disable CS0067 // Unused Event
        public event EventHandler CanExecuteChanged;
#pragma warning restore CS0067

        public DelegateCommand(params object[] parameters) : this(typeof(void), parameters)
        {

        }

        public DelegateCommand(Type returnType, params object[] parameters)
        {
            del = ReflectionUtility.CreateDelegate(GetType(), this);
            this.parameters = parameters ?? new object[0];
            CheckDelegateParameters();
        }

        public DelegateCommand(Delegate del, params object[] parameters)
        {
            this.del = del;
            this.parameters = parameters ?? new object[0];
            CheckDelegateParameters();
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute()
        {
            Execute(null);
        }

        public void Execute(object parameter)
        {
            var param = parameters;
            if (usesAdditionalParameter)
            {
                CheckAdditionalParameter(ref parameter);
                param = param.Concat(new [] { parameter }).ToArray();
            }
            Result = del?.DynamicInvoke(param);
        }

        public static void Run<T>(params object[] args) where T : DelegateCommand
        {
            var command = Activator.CreateInstance(typeof(T), args) as DelegateCommand;
            command.Execute();
        }

        private void CheckAdditionalParameter(ref object parameter)
        {
            var addParam = del.Method.GetParameters().Last();
            var methodParamType = addParam.ParameterType;
            if (parameter == null)
            {
                if (methodParamType.IsValueType &&
                    Nullable.GetUnderlyingType(methodParamType) == null &&
                    !addParam.HasDefaultValue)
                {
                    throw new Exception("Cannot assign null to non-optional value type");
                }
                else
                {
                    parameter = addParam.DefaultValue;
                }
                return;
            }

            var providedParamType = parameter.GetType();

            if (!methodParamType.IsAssignableFrom(providedParamType))
            {
                throw new InvalidCastException($"Cannot assign value of type {providedParamType} to type {methodParamType}");
            }
        }

        private void CheckDelegateParameters()
        {
            var method = del.Method;
            var methodParameters = method.GetParameters();

            if (methodParameters.Length == parameters.Length + 1)
                usesAdditionalParameter = true;
            else if (methodParameters.Length != parameters.Length)
                throw new Exception();

            for (int i = 0; i < parameters.Length; i++)
            {
                var methodParam = methodParameters[i];
                var providedParam = parameters[i];

                var methodParamType = methodParam.ParameterType;

                if (providedParam == null)
                {
                    if (methodParamType.IsValueType && 
                        Nullable.GetUnderlyingType(methodParamType) == null)
                    {
                        throw new Exception("Cannot assign null to value type");
                    }
                    continue;
                }

                var providedParamType = providedParam.GetType();

                if (!methodParamType.IsAssignableFrom(providedParamType))
                {
                    throw new InvalidCastException($"Cannot assign value of type {providedParamType} to type {methodParamType}");
                }
            }
        }
    }
}