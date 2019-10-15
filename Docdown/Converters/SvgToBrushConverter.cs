using Docdown.Util;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Xml.Linq;

namespace Docdown.Converters
{
    public class SvgToBrushConverter : IValueConverter
    {
        private readonly GeometryConverter geometryConverter = new GeometryConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string svg)
            {
                var brush = new DrawingBrush();
                var group = new DrawingGroup();

                try
                {
                    var doc = Parse(svg);
                    var svgElement = doc.Root;
                    var elements = svgElement.Descendants();

                    foreach (var node in elements)
                    {
                        group.Children.Add(FromElement(node));
                    }

                    brush.Drawing = group;
                    return brush;
                }
                catch
                {
                    return null;
                }
            }
            else return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        private XDocument Parse(string text)
        {
            return XDocument.Parse(text);
        }

        private GeometryDrawing FromElement(XElement element)
        {
            switch (element.Name.LocalName)
            {
                case "path":
                    return ConvertPath(element);
            }
            return null;
        }

        private GeometryDrawing ConvertPath(XElement path)
        {
            var data = path.Attribute("d")?.Value;
            if (geometryConverter.ConvertFromString(data) is Geometry geometry)
            {
                double.TryParse(path.Attribute("stroke-width")?.Value, out var strokeWidth);
                var fill = GetColorBrush(path, "fill");
                var stroke = GetColorBrush(path, "stroke");
                var pen = stroke != null && strokeWidth > 0 ? new Pen(stroke, strokeWidth) : null;
                return new GeometryDrawing(fill, pen, geometry);
            }
            else
            {
                return null;
            }
        }

        private SolidColorBrush GetColorBrush(XElement path, string attribute)
        {
            var value = path.Attribute(attribute)?.Value;
            return UIUtility.GetColorBrush(value);
        }
    }
}
