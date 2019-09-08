using Docdown.Util;
using Docdown.ViewModel.Commands;
using System;
using System.Windows.Documents;
using System.Windows.Input;
using System.Xml.Linq;

namespace Docdown.ViewModel
{
    public class WizardItemViewModel : ObservableObject
    {
        public string Name { get; set; }
        public string Content { get; set; }
        public FlowDocument Preview { get; set; }
        public WorkspaceViewModel Workspace { get; }

        public WizardItemViewModel(WorkspaceViewModel workspace)
        {
            Workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));
        }

        public override string ToString()
        {
            return Name;
        }

        public ICommand InsertTextCommand => new ActionCommand(InsertText);

        private void InsertText()
        {
            Workspace?.SelectedItem?.InsertTextAtPosition(Content);
        }

        public static WizardItemViewModel FromXml(WorkspaceViewModel workspace, XElement xml)
        {
            if (workspace is null)
                throw new ArgumentNullException(nameof(workspace));
            if (xml is null)
                throw new ArgumentNullException(nameof(xml));

            var item = new WizardItemViewModel(workspace)
            {
                Name = xml.Attribute(nameof(Name))?.Value,
                Content = xml.Element(nameof(Content))?.FirstNode?.ToString().Trim()
            };
            item.Preview = XamlUtility.ParseDocument(xml.Element(nameof(Preview))?.FirstNode);
            return item;
        }
    }
}