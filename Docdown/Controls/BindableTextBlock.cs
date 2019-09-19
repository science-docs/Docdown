using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Docdown.Controls
{
    public class BindableTextBlock : TextBlock
    {
        public static readonly DependencyProperty InlineProperty = DependencyProperty.Register(nameof(Inline), typeof(Inline), typeof(BindableTextBlock), new PropertyMetadata(null, InlineChanged));

        public Inline Inline
        {
            get => (Inline)GetValue(InlineProperty);
            set => SetValue(InlineProperty, value);
        }

        private static void InlineChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is BindableTextBlock textBlock)
            {
                textBlock.Inlines.Clear();
                if (e.NewValue is Inline inline)
                {
                    textBlock.Inlines.Add(inline);
                }
            }
        }
    }
}
