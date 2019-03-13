using Docdown.Controls;
using Docdown.ViewModel.Commands;
using System;
using System.Text.RegularExpressions;
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

        public string ReplaceText
        {
            get => replaceText;
            set => Set(ref replaceText, value);
        }

        public bool IsExpanded
        {
            get => expanded;
            set => Set(ref expanded, value);
        }

        public int SearchIndex { get; private set; }

        public bool CaseSensitive { get; set; }

        public ICommand SearchCommand => new ActionCommand(Search);
        public ICommand ShowCommand => new ActionCommand(ShowSearch);
        public ICommand HideCommand => new ActionCommand(HideSearch);
        public ICommand ReplaceCommand => new ActionCommand(Replace);
        public ICommand ReplaceAllCommand => new ActionCommand(ReplaceAll);

        private string searchText;
        private string replaceText;
        private string actualSearch;
        private string fullText;
        private bool visible;
        private bool expanded;

        public SearchViewModel(IEditor editor)
        {
            Editor = editor ?? throw new ArgumentNullException(nameof(editor));
            Editor.Editor.TextChanged += TextChanged;
        }

        private void TextChanged(object sender, EventArgs e)
        {
            fullText = null;
            SearchIndex = -1;
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
            else
            {
                SearchIndex = -1;
            }
        }

        private void Search()
        {
            if (string.IsNullOrEmpty(actualSearch))
            {
                return;
            }

            if (fullText is null)
            {
                fullText = Editor.Editor.Text;

                if (!CaseSensitive)
                {
                    fullText = fullText.ToLower();
                }
            }

            int index = fullText.IndexOf(actualSearch, SearchIndex == -1 ? 0 : SearchIndex);

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

        private void Replace()
        {
            if (string.IsNullOrEmpty(actualSearch))
            {
                return;
            }

            if (SearchIndex == -1)
            {
                Search();
            }
            else
            {
                ReplaceSelection();
                Search();
            }
        }

        private void ReplaceAll()
        {
            if (string.IsNullOrEmpty(actualSearch))
            {
                return;
            }

            Editor.Editor.Text = Regex.Replace(Editor.Editor.Text, actualSearch, replaceText, CaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);
        }

        private void SetSelection(int index)
        {
            try
            {
                SearchIndex = index + actualSearch.Length;
                Editor.Editor.SelectionLength = 0;
                Editor.Editor.SelectionStart = index;
                Editor.Editor.SelectionLength = actualSearch.Length;
                Editor.Editor.TextArea.Caret.Offset = index;
                Editor.Editor.TextArea.Caret.BringCaretToView();
            }
            catch
            {
                // Selection errored out
                // this is fine
            }
        }

        private void ReplaceSelection()
        {
            Editor.Editor.TextArea.Selection.ReplaceSelectionWithText(ReplaceText);
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
            actualSearch = SearchText ?? string.Empty;
            if (!CaseSensitive)
            {
                actualSearch = actualSearch.ToLower();
            }
        }
    }
}
