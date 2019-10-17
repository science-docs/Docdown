using Docdown.Model;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Docdown.ViewModel
{
    public class NewFileViewModel : ObservableList<NewFile>
    {
        public NewFile Selected
        {
            get => selected;
            set => Set(ref selected, value);
        }

        public string Name
        {
            get => name;
            set => Set(ref name, value);
        }

        private NewFile selected;
        private string name;


        public NewFileViewModel()
        {

        }

        [ChangeListener(nameof(Selected))]
        private void SelectedChanged()
        {
            if (selected != null)
            {
                Name = $"{selected.Name.ToLower()}.{selected.Extension}";
            }
        }

        public async Task LoadAsync()
        {
            var asm = typeof(NewFileViewModel).Assembly;
            var names = asm.GetManifestResourceNames().Where(e => e.StartsWith("Docdown.Resources.Templates."));

            foreach (var name in names)
            {
                var extension = Path.GetExtension(name).Substring(1);
                var pre = Path.GetFileNameWithoutExtension(name);
                var fileName = Path.GetExtension(pre).Substring(1);

                var file = new NewFile
                {
                    Name = fileName,
                    Extension = extension
                };

                using (var ms = new MemoryStream())
                {
                    using (var rs = asm.GetManifestResourceStream(name))
                    {
                        await rs.CopyToAsync(ms);
                    }
                    file.Content = ms.ToArray();
                }
                Add(file);
            }
            if (Count > 0)
            {
                Selected = this[0];
            }
        }
    }
}
