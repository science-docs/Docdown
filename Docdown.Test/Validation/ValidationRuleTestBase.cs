using System.Windows.Controls;

namespace Docdown.Validation.Test
{
    public class ValidationRuleTestBase<T> where T: ValidationRule, new()
    {
        public T Rule { get; } = new T();
    }
}
