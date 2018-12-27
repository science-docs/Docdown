using Docdown.Util;
using Docdown.ViewModel;
using System;
using System.Windows.Controls;

namespace Docdown.Controls
{
    public partial class EditorAndViewer : UserControl
    {
        public EditorAndViewer()
        {
            this.AddHandler(nameof(WorkspaceViewModel.SelectedItemText), WorkspaceTextChanged);
            InitializeComponent();
        }

        public void SetText(string text)
        {
            Editor.Text = text;
            Editor.Redraw();
        }

        private void EditorTextChanged(object sender, EventArgs e)
        {
            var workspace = DataContext as WorkspaceViewModel;
            workspace.SelectedItemText = Editor.Text;
        }

        private void WorkspaceTextChanged()
        {
            if (DataContext is WorkspaceViewModel workspace)
            {
                Editor.FileName = workspace.SelectedItem.Data.FileSystemInfo.FullName;
                Editor.Text = workspace.SelectedItemText;
            }
        }
    }
}
