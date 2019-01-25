using Docdown.Util;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Xml.Linq;

namespace Docdown.ViewModel
{
    public class WizardViewModel : ObservableObject
    {
        public WorkspaceViewModel Workspace { get; }
        public IEnumerable<WizardItemViewModel> Items => items;
        public WizardItemViewModel SelectedItem
        {
            get => selectedItem;
            set => Set(ref selectedItem, value);
        }

        private readonly WizardItemViewModel[] items;
        private WizardItemViewModel selectedItem;

        public WizardViewModel(WorkspaceViewModel workspace)
        {
            Workspace = workspace;

            items = CreateItems();
            selectedItem = items.FirstOrDefault();
        }

        private WizardItemViewModel[] CreateItems()
        {
            List<WizardItemViewModel> items = new List<WizardItemViewModel>();

            var asm = typeof(WizardViewModel).Assembly;
            var names = asm.GetManifestResourceNames();

            foreach (var wizard in names.Where(e => e.StartsWith("Docdown.Resources.Wizards.") && e.EndsWith(".xml")))
            {
                var resource = asm.GetManifestResourceStream(wizard);
                var xdocument = XDocument.Load(resource);
                if (XamlUtility.ValidateWizard(xdocument))
                {
                    items.Add(WizardItemViewModel.FromXml(Workspace, xdocument.Root));
                }
            }
            items.Sort((a, b) => a.Name.CompareTo(b.Name));

            return items.ToArray();
        }
    }
}