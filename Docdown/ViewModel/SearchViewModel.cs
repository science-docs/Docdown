using Docdown.Controls;
using Docdown.ViewModel.Commands;
using System;
using System.Windows.Input;

namespace Docdown.ViewModel
{
    public class SearchViewModel : ObservableObject
    {
        public IEditor Editor { get; set; }

        public bool Visible
        {
            get => visible;
            set => Set(ref visible, value);
        }

        public string SearchText
        {
            get => searchText;
            set => Set(ref searchText, value);
        }

        public int SearchIndex { get; private set; }

        public bool CaseSensitive { get; set; }

        public ICommand SearchCommand => new ActionCommand(Search);
        public ICommand ShowCommand => new ActionCommand(ShowSearch);
        public ICommand HideCommand => new ActionCommand(HideSearch);

        private string searchText;
        private string actualSearch;
        private string fullText;
        private bool visible;

        public SearchViewModel(IEditor editor)
        {
            Editor = editor ?? throw new ArgumentNullException(nameof(editor));
            Editor.Editor.TextChanged += TextChanged;
        }

        private void TextChanged(object sender, EventArgs e)
        {
            fullText = null;
            SearchIndex = 0;
        }

        public void SelectSearchText()
        {
            var selection = Editor.Editor.TextArea.Selection;
            var text = selection.GetText();
            if (!string.IsNullOrEmpty(text))
            {
                SearchText = text;
                SearchIndex = Editor.Editor.SelectionStart + SearchText.Length;
            }
        }

        private void Search()
        {
            if (fullText == null)
            {
                fullText = Editor.Editor.Text;
                actualSearch = SearchText;

                if (!CaseSensitive)
                {
                    fullText = fullText.ToLower();
                    actualSearch = actualSearch.ToLower();
                }
            }

            int index = fullText.IndexOf(actualSearch, SearchIndex);

            if (index > -1)
            {
                SetSelection(index);
            }
            else if (SearchIndex > actualSearch.Length)
            {
                index = fullText.IndexOf(actualSearch, 0, SearchIndex - actualSearch.Length - 1);
                if (index > -1)
                {
                    SetSelection(index);
                }
            }
        }

        private void SetSelection(int index)
        {
            SearchIndex = index + actualSearch.Length;
            Editor.Editor.SelectionStart = index;
            Editor.Editor.SelectionLength = actualSearch.Length;
            Editor.Editor.TextArea.Caret.Offset = index;
            Editor.Editor.TextArea.Caret.BringCaretToView();
        }

        private void ShowSearch()
        {
            Visible = false;
            Visible = true;
            SelectSearchText();
        }

        private void HideSearch()
        {
            Visible = false;
            Editor.Editor.Focus();
        }

        [ChangeListener(nameof(SearchText))]
        private void SearchChanged()
        {
            actualSearch = SearchText;
            if (!CaseSensitive)
            {
                actualSearch = actualSearch.ToLower();
            }
        }
    }
}
