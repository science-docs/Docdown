using System;
using System.Globalization;
using System.Windows.Controls;
using Docdown.Util;

namespace Docdown.Validation
{
    public class FileNameValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value is string text)
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    return new ValidationResult(false, "File name cannot be empty");
                }

                if (!IOUtility.ContainsInvalidCharacters(text, out var c))
                {
                    return new ValidationResult(false, $"'{c}' is not a valid character");
                }
                return ValidationResult.ValidResult;
            }
            throw new InvalidCastException("Could not convert given value to string");
        }
    }
}
