using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PdfiumViewer.Wpf.Util
{
    internal static class BitmapUtility
    {
        public static double DpiX { get; }
        public static double DpiY { get; }

        public const double DefaultDpiX = 96;
        public const double DefaultDpiY = 96;

        static BitmapUtility()
        {
            var dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
            var dpiYProperty = typeof(SystemParameters).GetProperty("Dpi", BindingFlags.NonPublic | BindingFlags.Static);

            DpiX = (int)dpiXProperty.GetValue(null, null);
            DpiY = (int)dpiYProperty.GetValue(null, null);
        }

        public static System.Windows.Size ConvertSize(System.Windows.Size size)
        {
            var dpiXRatio = DefaultDpiX / DpiX;
            var dpiYRatio = DefaultDpiY / DpiY;
            return new System.Windows.Size(size.Width / dpiXRatio, size.Height / dpiYRatio);
        }

        public static BitmapSource ToBitmapSource(Image image)
        {
            return ToBitmapSource(image as Bitmap);
        }
        
        public static BitmapSource ToBitmapSource(Bitmap gdiBitmap)
        {
            if (gdiBitmap is null) return null;

            System.Drawing.Imaging.BitmapData data = gdiBitmap.LockBits(new Rectangle(0, 0, 
                gdiBitmap.Width, gdiBitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, gdiBitmap.PixelFormat);

            BitmapSource bs = BitmapSource.Create(gdiBitmap.Width, 
                gdiBitmap.Height, DpiX, DpiY, PixelFormats.Bgra32, null,
                data.Scan0, data.Stride * gdiBitmap.Height, data.Stride);

            gdiBitmap.UnlockBits(data);
            bs.Freeze();
            return bs;
        }

        public static BitmapSource ToBitmapSource(byte[] bytes, int width, int height, int dpiX, int dpiY)
        {
            var result = BitmapSource.Create(
                            width,
                            height,
                            dpiX,
                            dpiY,
                            PixelFormats.Bgra32,
                            null /* palette */,
                            bytes,
                            width * 4 /* stride */);
            result.Freeze();

            return result;
        }

        public static void Save(this BitmapSource image, string filePath)
        {
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));

            using (var fileStream = File.OpenWrite(filePath))
            {
                encoder.Save(fileStream);
            }
        }
    }
}