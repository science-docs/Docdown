using PdfiumViewer;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Docdown.Converters
{
    public class PathToPdfConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string path)
            {
                return PdfDocument.Load(path);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
