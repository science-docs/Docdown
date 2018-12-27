using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace PdfiumViewer.Wpf.Util
{
    internal class ScrollBarVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Orientation orientation &&
                Enum.TryParse<Orientation>(parameter as string, out var param) &&
                orientation == param)
            {
                return ScrollBarVisibility.Visible;
            }
            return ScrollBarVisibility.Disabled;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
