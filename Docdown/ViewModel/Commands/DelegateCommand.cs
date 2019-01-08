using System;
using System.Linq;
using System.Windows.Input;

namespace Docdown.ViewModel.Commands
{
    public class DelegateCommand : ICommand
    {
        private readonly Delegate del;
        private readonly object[] parameters;
        private bool usesAdditionalParameter = false;

#pragma warning disable CS0067 // Unused Event
        public event EventHandler CanExecuteChanged;
#pragma warning restore CS0067

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
                param = param.Concat(new object[] { parameter }).ToArray();
            }
            del?.DynamicInvoke(param);
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
                    throw new Exception($"Cannot assign value of type {providedParamType} to type {methodParamType}");
                }
            }
        }
    }
}