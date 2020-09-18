using PdfiumViewer.Wpf.Util;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace PdfiumViewer.Wpf
{
    internal partial class PageView
    {
        private bool alreadySet = false;
        private Size? lastSize;
        private readonly Action debounced;

        public PageView()
        {
            InitializeComponent();
            LayoutUpdated += PageLayoutUpdated;
            debounced = UIUtility.Debounce(FullUpdate, 1000);
        }

        private void PageLayoutUpdated(object sender, EventArgs e)
        {
            Update();
        }

        protected override Size MeasureOverride(Size constraint)
        {
            if (HasSizeChanged(constraint))
            {
                LinkCanvas.Children.Clear();
                debounced();
            }

            if (DataContext is PageViewModel page)
            {
                var size = page.Resize(constraint);
                base.MeasureOverride(size);
                return size;
            }
            return Size.Empty;
        }

        private void FullUpdate()
        {
            Dispatcher.InvokeAsync(() =>
            {
                alreadySet = false;
                Update();
            });
        }

        private void Update()
        {
            var isVisible = PageIsVisible();
            if (!alreadySet && isVisible)
            {
                alreadySet = true;
                BeginSetImage();
            }
            else if (alreadySet && !isVisible)
            {
                alreadySet = false;
                UnsetImage();
            }
        }

        private bool HasSizeChanged(Size constraint)
        {
            if (lastSize is null)
            {
                lastSize = constraint;
                return false;
            }
            var temp = lastSize.Value;
            lastSize = constraint;
            return temp != constraint;
        }

        private bool PageIsVisible()
        {
            var scrollViewer = this.GetParentByType<ScrollViewer>();

            if (scrollViewer is null)
            {
                return true;
            }
            var zero = new Point(0, 0);

            var childTransform = TransformToAncestor(scrollViewer);
            
            var rectangle = childTransform.TransformBounds(new Rect(zero, DesiredSize));
            var result = Rect.Intersect(new Rect(zero, scrollViewer.RenderSize), rectangle);

            return result != Rect.Empty;
        }

        private void BeginSetImage()
        {
            if (DataContext is PageViewModel page)
            {
                var links = LinkCanvas.Children.Count == 0;
                Task.Run(() => Render(page, links));
            }
        }

        private async Task Render(PageViewModel page, bool setLinks)
        {
            var image = await page.Render();
            await SetImage(image);
            if (setLinks)
            {
                await SetLinks(page);
            }
        }

        private async Task SetImage(BitmapSource image)
        {
            await Dispatcher.InvokeAsync(() => Img.Source = image);
        }

        private async Task SetLinks(PageViewModel page)
        {
            var size = new System.Drawing.Size((int)ActualWidth, (int)ActualHeight);
            var pageSize = page.Document.GetPageSize(page.Page);
            //var links = page.Document.GetPageLinks(page.Page, size);
            //foreach (var link in links.Links)
            //{
            //    var translated = page.Document.RectangleFromPdf(page.Page, link.Bounds);
            //    System.Drawing.RectangleF rectF = translated;
            //    RecalculateRect(pageSize, page.RenderSize, ref rectF);
            //    await Dispatcher.InvokeAsync(() => CreateRectangle(rectF, link.TargetPage ?? 0));
            //}
        }

        private void RecalculateRect(System.Drawing.SizeF pageSize, Size controlSize, ref System.Drawing.RectangleF linkRect)
        {
            var factor = (float)(controlSize.Width / pageSize.Width);
            linkRect.Height *= factor;
            linkRect.Width *= factor;
            linkRect.Y *= factor;
            linkRect.X *= factor;
        }

        private static readonly Brush green = new SolidColorBrush(new Color { G = 255, A = 128 });

        private void CreateRectangle(System.Drawing.RectangleF rect, object page)
        {
            var rectangle = new Rectangle
            {
                Width = rect.Width,
                Height = rect.Height,
                Fill = Brushes.Transparent,
                Stroke = green,
                StrokeThickness = 1,
                Cursor = System.Windows.Input.Cursors.Hand,
                Tag = page
            };
            rectangle.MouseDown += LinkMouseDown;
            Canvas.SetLeft(rectangle, rect.X);
            Canvas.SetTop(rectangle, rect.Y);
            LinkCanvas.Children.Add(rectangle);
        }

        private void LinkMouseDown(object sender, EventArgs e)
        {
            var rectangle = sender as Rectangle;
            var page = (int)rectangle.Tag;
            var viewer = rectangle.GetParentByType<PdfViewer>();
            viewer.ScrollTo(page);
        }

        private void UnsetImage()
        {
            LinkCanvas.Children.Clear();
            Img.Source = null;
        }
    }
}