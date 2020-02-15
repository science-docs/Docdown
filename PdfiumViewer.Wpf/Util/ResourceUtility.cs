using System;
using System.Windows;

namespace PdfiumViewer.Wpf.Util
{
    internal static class ResourceUtility
    {
        private static readonly ResourceDictionary dictionary = new ResourceDictionary();

        static ResourceUtility()
        {
            AddDictionary("Style");
        }

        public static T TryFindResource<T>(string name)
        {
            if (dictionary.Contains(name) && dictionary[name] is T obj)
            {
                return obj;
            }
            else
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
