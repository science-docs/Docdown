using System.Globalization;
using System.Windows.Controls;

namespace Docdown.Validation
{
    public class StaticCaptchaValidationRule : ValidationRule
    {
        public static ValidationResult Result { get; set; }

        public StaticCaptchaValidationRule() : base(ValidationStep.UpdatedValue, true)
        {

        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return Result ?? ValidationResult.ValidResult;
        }
    }
}
