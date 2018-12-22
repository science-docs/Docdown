using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PdfiumViewer.Wpf.Util
{
    internal static class ResourceUtility
    {
        private static ResourceDictionary dictionary;

        static ResourceUtility()
        {
            dictionary = new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/PdfiumViewer.Wpf;component/Style.xaml")
            };
        }

        public static T TryFindResource<T>(string name)
        {
            try
            {
                return (T)dictionary[name];
            }
            catch
            {
                return default;
            }
        }
    }
}
