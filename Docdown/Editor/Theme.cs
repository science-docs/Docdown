using Docdown.Util;
using Docdown.ViewModel;
using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace Docdown.Editor
{
    public class ThemeChangedEventArgs : EventArgs
    {
        public Theme Theme { get; set; }
    }

    public class Theme : ObservableObject
    {
        private string _name = "Zenburn";
        //private string _editorBackground = "#404040";
        //private string _editorForeground = "#ccc";
        private string _spellCheckError = "#f00";

        public IEnumerable<Highlight> Highlights => highlights.Values;

        private readonly Dictionary<string, Highlight> highlights = new Dictionary<string, Highlight>();

        public Highlight this[string name]
        {
            get => highlights[name];
            set => highlights[name] = value;
        }

        public void Add(Highlight highlight)
        {
            highlights[highlight.Name] = highlight;
        }

        public static Brush BlueBrush { get; } = UIUtility.GetColorBrush("#3786D4");
        public string Name
        {
            get => _name;
            set => Set(ref _name, value);
        }

        public Brush Info { get; set; } = Brushes.Blue;
        public Brush Warning { get; set; } = Brushes.Yellow;
        public Brush Error { get; set; } = Brushes.Red;

        public string SpellCheckError
        {
            get => _spellCheckError;
            set => Set(ref _spellCheckError, value);
        }
    }
}
