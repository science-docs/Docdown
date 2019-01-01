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
            InitializeComponent();
        }

        public void SetText(string text)
        {
            Editor.Text = text;
            Editor.Redraw();
        }

        public string GetText()
        {
            return Editor.Text;
        }

        private void EditorTextChanged(object sender, EventArgs e)
        {
            //var workspace = DataContext as WorkspaceViewModel;
            //workspace.SelectedItemText = Editor.Text;
        }

        private void WorkspaceTextChanged()
        {
            if (DataContext is WorkspaceViewModel workspace && workspace.SelectedItem != null)
            {
                Editor.FileName = workspace.SelectedItem.Data.FileSystemInfo.FullName;
                Editor.Text = workspace.SelectedItemText;
            }
        }
    }
}
