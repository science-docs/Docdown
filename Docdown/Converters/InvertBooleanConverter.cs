using System;
using System.Globalization;
using System.Windows.Data;

namespace Docdown.Converters
{
    public class InvertBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
            {
                return !b;
            }
            else if (parameter is bool)
            {
                return parameter;
            }
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
            {
                return !b;
            }
            else if (parameter is bool)
            {
                return parameter;
            }
            return false;
        }
    }
}
