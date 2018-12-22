using Docdown.Model;
using Docdown.Util;
using Docdown.ViewModel;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Docdown
{
    public partial class MainWindow
    {
        WorkspaceViewModel workspaceViewModel;

        public MainWindow()
        {
            InitializeComponent();
            var workspace = new Workspace("H:\\Mega\\Arbeit");
            workspaceViewModel = new WorkspaceViewModel(workspace);
            workspaceViewModel.SelectedItemTextChanged += WorkspaceTextChanged;
            DataContext = WorkspaceView.DataContext = workspaceViewModel;

            Editor.TextChanged += EditorTextChanged;
        }

        private void EditorTextChanged(object sender, EventArgs e)
        {
            workspaceViewModel.SelectedItemText = Editor.Text;
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            string text = Editor.Text;
            Viewer.StartLoad();
            Viewer.DismissError();
            Task.Run(() =>
            {
                string tempFile = Path.GetTempFileName();
                File.WriteAllText(tempFile, text);
                try
                {
                    var response = WebUtility.MultipartFormDataPost("http://localhost:3030/convert", "Docdown",
                    MultipartFormParameter.CreateField("from", "markdown"),
                    MultipartFormParameter.CreateField("to", "pdf"),
                    MultipartFormParameter.CreateField("template", "eisvogel"),
                    MultipartFormParameter.CreateFile("content", tempFile));

                    string tempFinalFile = Path.GetTempFileName();
                    using (var responseStream = response.GetResponseStream())
                    {
                        using (var fs = new FileStream(tempFinalFile, FileMode.Open))
                        {
                            responseStream.CopyTo(fs);
                        }
                    }
                    Viewer.Navigate(tempFinalFile);
                }
                catch
                {
                    Viewer.ShowError("Client could not connect to server");
                }
            });
        }

        private void WorkspaceTextChanged(object sender, EventArgs e)
        {
            Editor.FileName = workspaceViewModel.SelectedItem.Data.FileSystemInfo.FullName;
            Editor.Text = workspaceViewModel.SelectedItemText;
        }
    }
}