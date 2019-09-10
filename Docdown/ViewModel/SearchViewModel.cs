using Docdown.Editor;
using Docdown.Util;
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

        public bool UseRegex { get; set; }

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
        private Regex searchRegex;

        public SearchViewModel(IEditor editor)
        {
            Editor = editor ?? throw new ArgumentNullException(nameof(editor));
            Editor.Editor.TextChanged += TextChanged;
        }

        private void TextChanged(object sender, EventArgs e)
        {
            fullText = null;
            SearchIndex = Math.Min(SearchIndex, Editor.Editor.Document.TextLength);
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

            fullText = fullText ?? Editor.Editor.Text;

            var match = searchRegex.Match(fullText, SearchIndex == -1 ? 0 : SearchIndex);

            if (match.Success)
            {
                SetSelection(match);
            }
            else if (SearchIndex > -1)
            {
                match = searchRegex.Match(fullText, 0);
                if (match.Success)
                {
                    SetSelection(match);
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

        private void SetSelection(Match match)
        {
            try
            {
                SearchIndex = match.Index + 1;
                Editor.Editor.SelectionLength = 0;
                Editor.Editor.SelectionStart = match.Index;
                Editor.Editor.SelectionLength = match.Length;
                Editor.Editor.ScrollTo(match.Index);
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
            RegexOptions options = RegexOptions.None;
            if (!CaseSensitive)
            {
                options |= RegexOptions.IgnoreCase;
            }
            if (!UseRegex)
            {
                actualSearch = Regex.Escape(actualSearch);
            }
            searchRegex = new Regex(actualSearch, options);
        }
    }
}
