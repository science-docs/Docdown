using System.Windows.Input;

namespace Docdown.Controls
{
    public interface IWrappedView
    {
        ICommand CloseCommand { get; }
    }
}
