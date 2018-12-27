using System;
using System.Windows;

namespace PdfiumViewer.Wpf.Util
{
    internal static class ResourceUtility
    {
        private static readonly ResourceDictionary dictionary;

        static ResourceUtility()
        {
            dictionary = new ResourceDictionary();
            AddDictionary("Style");
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

        private static void AddDictionary(string name)
        {
            var resourceDic = new ResourceDictionary
            {
                Source = new Uri($"pack://application:,,,/PdfiumViewer.Wpf;component/{name}.xaml")
            };

            dictionary.MergedDictionaries.Add(resourceDic);
        }
    }
}
