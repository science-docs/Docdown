using System;
using System.Globalization;
using System.Windows.Data;

namespace Docdown.Converters
{
    public class StringLengthToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var text = value?.ToString() ?? string.Empty;
            return text.Length > 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
