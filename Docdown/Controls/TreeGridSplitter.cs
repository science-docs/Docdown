using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Docdown.Controls
{
    public class TreeGridSplitter : Thumb
    {
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(TreeGridSplitter), new FrameworkPropertyMetadata(Orientation.Horizontal, FrameworkPropertyMetadataOptions.AffectsRender, OrientationChanged));

        public Orientation Orientation
        {
            get => (Orientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }
        public TreeNode Node { get; set; }

        private bool dragging;

        static TreeGridSplitter()
        {
            EventManager.RegisterClassHandler(typeof(TreeGridSplitter), DragStartedEvent, new DragStartedEventHandler(OnDragStarted));
            EventManager.RegisterClassHandler(typeof(TreeGridSplitter), DragDeltaEvent, new DragDeltaEventHandler(OnDragDelta));
            EventManager.RegisterClassHandler(typeof(TreeGridSplitter), DragCompletedEvent, new DragCompletedEventHandler(OnDragCompleted));
        }

        public TreeGridSplitter()
        {
            Background = Brushes.Transparent;
            Cursor = Cursors.SizeWE;
        }

        private static void OnDragStarted(object sender, DragStartedEventArgs e)
        {
            TreeGridSplitter splitter = sender as TreeGridSplitter;
            splitter.OnDragStarted();
        }

        private void OnDragStarted()
        {
            dragging = true;
        }

        private static void OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            TreeGridSplitter splitter = sender as TreeGridSplitter;
            splitter.OnDragDelta(e);
        }

        private void OnDragDelta(DragDeltaEventArgs e)
        {
            if (dragging)
            {
                double horizontalChange = e.HorizontalChange;
                double verticalChange = e.VerticalChange;
                MoveSplitter(horizontalChange, verticalChange);
            }
        }

        private static void OnDragCompleted(object sender, DragCompletedEventArgs e)
        {
            TreeGridSplitter splitter = sender as TreeGridSplitter;
            splitter.OnDragCompleted();
        }

        // Thumb dragging finished
        private void OnDragCompleted()
        {
            dragging = false;
        }

        private void MoveSplitter(double horizontal, double vertical)
        {
            bool isHorizontal = Orientation == Orientation.Horizontal;

            if (isHorizontal)
            {
                double width = Node.Size.Width;
                double widthA = width * Node.Distribution;
                double newWidthA = widthA + horizontal;
                double newDist = newWidthA / width;
                Node.Distribution = newDist;
            }
            else
            {
                double height = Node.Size.Height;
                double heightA = height * Node.Distribution;
                double newHeightA = heightA + vertical;
                double newDist = newHeightA / height;
                Node.Distribution = newDist;
            }
        }

        private static void OrientationChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is TreeGridSplitter splitter && e.NewValue is Orientation orientation)
            {
                splitter.Cursor = orientation == Orientation.Horizontal ? Cursors.SizeWE : Cursors.SizeNS;
            }
        }
    }
}
