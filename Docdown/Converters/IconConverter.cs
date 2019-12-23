using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Docdown.Converters
{
    public class IconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(Application.Current.TryFindResource(value) is Brush))
            {
                return null;
            }

            var rect = new Rectangle
            {
                Width = 16,
                Height = 16,
                Fill = (Brush)Application.Current.TryFindResource(value)
            };
            return rect;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
