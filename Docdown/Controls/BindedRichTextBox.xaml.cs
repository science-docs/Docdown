using System.Windows;
using System.Windows.Documents;

namespace Docdown.Controls
{
    public partial class BindedRichTextBox
    {
        public FlowDocument Document
        {
            get => (FlowDocument)GetValue(DocumentProperty);
            set => SetValue(DocumentProperty, value);
        }

        public static readonly DependencyProperty DocumentProperty =
            DependencyProperty.Register("Document", typeof(FlowDocument), typeof(BindedRichTextBox), new PropertyMetadata(OnDocumentChanged));

        private static void OnDocumentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BindedRichTextBox control = (BindedRichTextBox)d;
            control.RTB.Document = control.cache = e.NewValue as FlowDocument ?? new FlowDocument();
        }

        private FlowDocument cache;

        public BindedRichTextBox()
        {
            InitializeComponent();
            Unloaded += OnUnloaded;
            Loaded += OnLoaded;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            RTB.Document = new FlowDocument();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (cache != null)
            {
                RTB.Document = cache;
            }
        }
    }
}