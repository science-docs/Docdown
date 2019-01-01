using Docdown.Model;
using Docdown.Util;
using Docdown.ViewModel;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Docdown.Controls
{
    // TODO: Remove in near future
    [Obsolete]
    public class SelectorView : ContentControl
    {
        public SelectorView()
        {
            this.AddHandler(nameof(WorkspaceViewModel.SelectedItem), SelectedItemChanged);
        }

        private readonly Dictionary<WorkspaceItemType, object> viewCache
            = new Dictionary<WorkspaceItemType, object>();

        private void SelectedItemChanged()
        {
            var workspace = DataContext as WorkspaceViewModel;
            var selectedItem = workspace.Data.SelectedItem;
            if (selectedItem != null)
            {
                switch (selectedItem.Type)
                {
                    case WorkspaceItemType.Pdf:
                        ShowPdf(workspace, selectedItem);
                        break;
                    case WorkspaceItemType.Latex:
                    case WorkspaceItemType.Markdown:
                        ShowMdEditorAndPdf(workspace, selectedItem);
                        break;
                    default:
                        Content = null;
                        break;
                }
            }
            else
            {
                Content = null;
            }
        }

        private void ShowPdf(WorkspaceViewModel workspace, WorkspaceItem item)
        {
            if (!viewCache.TryGetValue(WorkspaceItemType.Pdf, out var view))
            {
                view = new DocumentViewer();
                viewCache[WorkspaceItemType.Pdf] = view;
            }
            var docViewer = view as DocumentViewer;
            Content = docViewer;
            try
            {
                docViewer.Navigate(item.FileSystemInfo.FullName);
            }
            catch (Exception e)
            {
                workspace.ErrorMessage = ErrorUtility.GetErrorMessage(e);
            }
        }

        private void ShowMdEditorAndPdf(WorkspaceViewModel workspace, WorkspaceItem item)
        {
            if (!viewCache.TryGetValue(WorkspaceItemType.Markdown, out var view))
            {
                view = new EditorAndViewer();
                viewCache[WorkspaceItemType.Markdown] = view;
            }
            var editorAndViewer = view as EditorAndViewer;
            Content = editorAndViewer;
            this.Delay(100, () => editorAndViewer.SetText(workspace.SelectedItemText));
        }
    }
}