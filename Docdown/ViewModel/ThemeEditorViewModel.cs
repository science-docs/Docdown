using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Docdown.Editor;
using Docdown.ViewModel.Commands;

namespace Docdown.ViewModel
{
    public class ThemeEditorViewModel : ObservableObject
    {
        public ObservableCollection<Theme> Themes { get; } = new ObservableCollection<Theme>();

        public Theme SelectedTheme
        {
            get => selectedTheme;
            set => Set(ref selectedTheme, value);
        }

        public Highlight SelectedHighlight
        {
            get => selectedHighlight;
            set => Set(ref selectedHighlight, value);
        }

        public ICommand SaveCommand => new ActionCommand(Save);


        private Theme selectedTheme;
        private Highlight selectedHighlight;

        public ThemeEditorViewModel()
        {
            foreach (var theme in ThemePersistance.LoadAll().OrderByDescending(e => e.Name))
            {
                Themes.Add(theme);
            }
            SelectedTheme = Themes[0];
        }

        public void Save()
        {
            ThemePersistance.SaveAll(Themes);
        }
    }
}
