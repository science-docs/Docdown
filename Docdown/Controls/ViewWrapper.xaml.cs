using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Docdown.Controls
{
    //[ContentProperty("Item")]
    public partial class ViewWrapper : UserControl
    {
        public static DependencyProperty ItemProperty = DependencyProperty.Register("Item", typeof(FrameworkElement), typeof(ViewWrapper));

        public FrameworkElement Item
        {
            get => (FrameworkElement)GetValue(ItemProperty);
            set => SetValue(ItemProperty, value);
        }

        public ViewWrapper()
        {
            InitializeComponent();
        }
    }
}
