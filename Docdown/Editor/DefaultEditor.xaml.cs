using System.Windows.Controls;
using Docdown.ViewModel;
using Docdown.ViewModel.Editing;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Folding;

namespace Docdown.Editor
{
    public partial class DefaultEditor : UserControl, IEditor
    {
        public TextEditor Editor => EditBox;

        private bool firstChange = false;

        private FoldingManager foldingManager;

        public DefaultEditor()
        {
            InitializeComponent();

            Editor.TextChanged += delegate
            {
                if (DataContext is WorkspaceItemViewModel item)
                {
                    item.Editor.Text = EditBox.Text;
                    if (firstChange)
                    {
                        item.HasChanged = true;
                    }
                    else
                    {
                        SetFirst(item);
                    }
                    var foldingStrategy = item.Editor.FoldingStrategy;
                    if (foldingStrategy != null)
                    {
                        foldingManager.UpdateFoldings(foldingStrategy.GenerateFoldings(), -1);
                    }
                }
                firstChange = true;
            };
        }

        private void SetFirst(WorkspaceItemViewModel item)
        {
            foreach (var lineTransformer in item.Editor.LineTransformers)
            {
                EditBox.TextArea.TextView.LineTransformers.Add(lineTransformer);
            }
            foreach (var backgroundRenderer in item.Editor.BackgroundRenderers)
            {
                EditBox.TextArea.TextView.BackgroundRenderers.Add(backgroundRenderer);
            }

            if (foldingManager == null && item.Editor.FoldingStrategy != null)
            {
                foldingManager = FoldingManager.Install(EditBox.TextArea);
            }
        }
    }
}
