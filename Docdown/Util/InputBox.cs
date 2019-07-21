using Docdown.ViewModel;
using Docdown.Windows;
using System.Windows;

namespace Docdown.Util
{
    public class InputBox
    {
        public static string Show(string title, string message, string pretext)
        {
            ReflectionUtility.EnsureMainThread();
            var viewModel = new InputBoxViewModel(title, message, pretext);
            InputWindow messageWindow = new InputWindow
            {
                DataContext = viewModel,
                Owner = Application.Current.MainWindow
            };
            if (messageWindow.ShowDialog().Value)
            {
                return viewModel.Text;
            }
            return null;
        }
    }
}
