﻿using Docdown.ViewModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Docdown.Controls
{
    public partial class WorkspaceView
    {
        public WorkspaceView()
        {
            InitializeComponent();
        }

        private void ViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is Explorer explorer && 
                e.NewValue is Explorer explorerItem)
            {
                explorer.Workspace.PreSelectedItem = explorerItem.WorkspaceItem;
            }
        }

        private void ViewSelectedMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && 
                DataContext is Explorer explorer)
            {
                explorer.Workspace.SelectedItem = explorer.Workspace.PreSelectedItem;
                Keyboard.ClearFocus();
            }
        }

        private void DropFromTreeView(object sender, DragEventArgs e)
        {
            if (DataContext is Explorer explorer && explorer.Children.Any())
            {
                HandleDrop(explorer.Children.First(), e);
            }
        }

        private void DropFromTreeViewItem(object sender, DragEventArgs e)
        {
            if (sender is FrameworkElement elem && elem.DataContext is Explorer explorer)
            {
                if (explorer.WorkspaceItem.IsFile)
                {
                    explorer = explorer.Parent;
                }
                HandleDrop(explorer, e);
            }
        }

        private void HandleDrop(Explorer explorer, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Note that you can have more than one file.
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                // Assuming you have one file that you care about, pass it off to whatever
                // handling code you have defined.
                var workspaceItem = explorer.WorkspaceItem;
                var workspace = workspaceItem.Workspace;
                workspace.IgnoreChange = true;
                var item = workspaceItem.Data;
                foreach (var name in files)
                {
                    var isFolder = Directory.Exists(name);
                    var newChild = isFolder ? item.CopyExistingFolder(name) : item.CopyExistingItem(name);
                    workspaceItem.AddChild(newChild);
                }
                workspace.IgnoreChange = false;
                e.Handled = true;
            }
        }
    }
}