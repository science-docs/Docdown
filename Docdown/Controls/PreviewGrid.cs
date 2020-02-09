using Docdown.ViewModel;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace Docdown.Controls
{
    public class PreviewGrid : Panel
    {
        public static readonly DependencyProperty ItemsSourceProperty 
            = DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(PreviewGrid), new PropertyMetadata(null, ItemsSourceChanged));

        public IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public bool ShowDefault => defaultItems.Count > 0;
        public bool ShowPreview => previewItems.Count > 0;

        private readonly ObservableCollection<WorkspaceItemViewModel> defaultItems
            = new ObservableCollection<WorkspaceItemViewModel>();
        private readonly ObservableCollection<WorkspaceItemViewModel> previewItems
            = new ObservableCollection<WorkspaceItemViewModel>();

        private readonly TabView defaultTab = new TabView();
        private readonly TabView previewTab = new TabView();

        public PreviewGrid()
        {
            defaultTab.ItemsSource = defaultItems;
            previewTab.ItemsSource = previewItems;
            defaultTab.SetBinding(Selector.SelectedItemProperty, new Binding("SelectedItem") { Mode = BindingMode.TwoWay });
            previewTab.SetBinding(Selector.SelectedItemProperty, new Binding("SelectedPreview") { Mode = BindingMode.TwoWay });
            Children.Add(defaultTab);
            Children.Add(previewTab);

            DataContextChanged += PreviewGrid_DataContextChanged;
        }

        private void PreviewGrid_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            defaultTab.DataContext = DataContext; 
            previewTab.DataContext = DataContext;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            bool both = ShowDefault && ShowPreview;
            double halfWidth = constraint.Width / 2 - 3;

            if (both)
            {
                var halfSize = new Size(halfWidth, constraint.Height);
                defaultTab.Measure(halfSize);
                previewTab.Measure(halfSize);
            }
            else if (ShowDefault)
            {
                defaultTab.Measure(constraint);
            }
            else if (ShowPreview)
            {
                previewTab.Measure(constraint);
            }

            return constraint;
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            bool both = ShowDefault && ShowPreview;
            double halfWidth = arrangeBounds.Width / 2 - 3;
            double halfX = halfWidth + 6;
            var rect = new Rect(arrangeBounds);

            if (both)
            {
                var halfSize = new Size(halfWidth, arrangeBounds.Height);
                var defRect = new Rect(halfSize);
                var previewRect = new Rect(new Point(halfX, 0), halfSize);
                defaultTab.Arrange(defRect);
                previewTab.Arrange(previewRect);
                
            }
            else if (ShowDefault)
            {
                defaultTab.Arrange(rect);
            }
            else if (ShowPreview)
            {
                previewTab.Arrange(rect);
            }

            return arrangeBounds;
        }

        private void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            if (oldValue is INotifyCollectionChanged oldChanged)
            {
                oldChanged.CollectionChanged -= ItemListChanged;
            }
            if (newValue is INotifyCollectionChanged newChanged)
            {
                newChanged.CollectionChanged += ItemListChanged;
            }
            ItemsChanged(newValue);
        }

        private void ItemsChanged(IEnumerable list)
        {
            var typedList = list.OfType<WorkspaceItemViewModel>();
            var removeDefaultList = defaultItems.Except(typedList);
            var removePreviewList = previewItems.Except(typedList);

            InvalidateMeasure();
            if (list == null)
                return;

            foreach (var item in typedList.Except(defaultItems.Concat(previewItems)))
            {
                if (item.IsPreview)
                {
                    previewItems.Add(item);
                }
                else
                {
                    defaultItems.Add(item);
                }
            }

            foreach (var remove in removeDefaultList)
            {
                defaultItems.Remove(remove);
            }
            foreach (var remove in removePreviewList)
            {
                previewItems.Remove(remove);
            }

            if (ShowDefault)
            {
                if (!Children.Contains(defaultTab))
                    Children.Add(defaultTab);
            }
            else
            {
                Children.Remove(defaultTab);
            }
            if (ShowPreview)
            {
                if (!Children.Contains(previewTab))
                    Children.Add(previewTab);
            }
            else
            {
                Children.Remove(previewTab);
            }
        }

        private void ItemListChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ItemsChanged(ItemsSource);
        }

        private void IdentifyItem(object item, ref bool preview, ref bool def)
        {
            if (item is WorkspaceItemViewModel workspaceItem)
            {
                preview |= workspaceItem.IsPreview;
                def |= !preview;
            }
        }

        private static void ItemsSourceChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObject is PreviewGrid grid)
            {
                grid.OnItemsSourceChanged(e.OldValue as IEnumerable, e.NewValue as IEnumerable);
            }
        }
    }
}
