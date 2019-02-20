using System.Windows;

namespace Docdown.Util
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
