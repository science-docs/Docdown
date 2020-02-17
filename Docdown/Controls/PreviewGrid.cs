using Docdown.ViewModel;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Docdown.Controls
{
    public class PreviewGrid : ContentControl
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
        private readonly TreeGrid treeGrid = new TreeGrid();
        private readonly TreeNode node = new TreeNode();
        private double ratio = 0.5;

        public PreviewGrid()
        {
            defaultTab.ItemsSource = defaultItems;
            previewTab.ItemsSource = previewItems;
            defaultTab.SetBinding(Selector.SelectedItemProperty, "SelectedItem");
            previewTab.SetBinding(Selector.SelectedItemProperty, "SelectedPreview");
            treeGrid.Tree = node;
            treeGrid.SplitterThickness = 6;
            Content = treeGrid;

            DataContextChanged += PreviewGrid_DataContextChanged;
        }

        private void PreviewGrid_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            defaultTab.DataContext = DataContext; 
            previewTab.DataContext = DataContext;
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
            var typedList = list.OfType<WorkspaceItemViewModel>().ToArray();
            var removeDefaultList = defaultItems.Except(typedList).ToArray();
            var removePreviewList = previewItems.Except(typedList).ToArray();

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
                if (!node.Contains(defaultTab))
                    node.AddA(defaultTab, Orientation.Horizontal, ratio);
            }
            else if (node.Contains(defaultTab))
            {
                ratio = node.Distribution;
                node.Remove(defaultTab);
            }
            if (ShowPreview)
            {
                if (!node.Contains(previewTab))
                    node.AddB(previewTab, Orientation.Horizontal, ratio);
            }
            else if (node.Contains(previewTab))
            {
                ratio = node.Distribution;
                node.Remove(previewTab);
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
