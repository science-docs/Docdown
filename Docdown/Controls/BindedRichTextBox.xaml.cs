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
            control.RTB.Document = e.NewValue as FlowDocument ?? new FlowDocument();
        }

        public BindedRichTextBox()
        {
            InitializeComponent();
        }
    }
}