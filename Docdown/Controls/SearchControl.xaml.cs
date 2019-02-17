using System.Windows;

namespace Docdown.Controls
{
    public partial class SearchControl
    {
        public SearchControl()
        {
            InitializeComponent();
            IsVisibleChanged += SearchVisibleChanged;
        }

        private void SearchVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible)
            {
                SearchBox.SelectAll();
                SearchBox.Focus();
            }
        }
    }
}
