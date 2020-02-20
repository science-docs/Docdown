using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Media;

namespace Docdown.Validation
{
    public class ColorValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value is string color)
            {
                try
                {
                    ColorConverter.ConvertFromString(color);
                }
                catch (FormatException)
                {
                    return new ValidationResult(false, "Could not convert to a color");
                }
            }
            return ValidationResult.ValidResult;
        }
    }
}
