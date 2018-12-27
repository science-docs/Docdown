using System.Windows;

namespace PdfiumViewer.Wpf.Util
{
    public static class ResourceUtility
    {
        public static T TryFindResource<T>(string name)
        {
            try
            {
                return (T)Application.Current.FindResource(name);
            }
            catch
            {
                return default;
            }
        }
    }
}
