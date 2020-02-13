using Docdown.ViewModel;
using System;
using System.Windows.Controls;

namespace Docdown.Controls
{
    public static class CommonControls
    {
        public static ViewWrapper WorkspaceExplorer => workspaceExplorer.Value;
        public static ViewWrapper Outline => outline.Value;
        public static PreviewGrid MainContent => main.Value;

        private static readonly Lazy<ViewWrapper> workspaceExplorer = new Lazy<ViewWrapper>(CreateWrapper<WorkspaceView>);
        private static readonly Lazy<ViewWrapper> outline = new Lazy<ViewWrapper>(CreateWrapper<OutlineView>);
        private static readonly Lazy<PreviewGrid> main = new Lazy<PreviewGrid>(CreateMainContent);

        private static ViewWrapper CreateWrapper<T>() where T : new()
        {
            var wrapper = new ViewWrapper();

            var item = new T();
            wrapper.Content = item;

            return wrapper;
        }

        private static PreviewGrid CreateMainContent()
        {
            var grid = new PreviewGrid
            {
                DataContext = AppViewModel.Instance.Workspace
            };
            //tab.SetBinding(FrameworkElement.DataContextProperty, "Workspace");
            grid.SetBinding(PreviewGrid.ItemsSourceProperty, "OpenItems");

            return grid;
        }

        public static void Layout(TreeGrid grid, CommonControlType type, Dock dock)
        {
            var tree = grid.Tree;
            if (tree == null)
            {
                tree = new TreeNode();
            }

            grid.Tree = tree;
        }
    }

    public enum CommonControlType
    {
        Main = 0,
        Explorer,
        Outline
    }
}
