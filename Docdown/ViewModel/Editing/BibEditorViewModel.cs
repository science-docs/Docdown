using Docdown.Editor;
using Docdown.Editor.BibTex;
using Docdown.Editor.Markdown;
using Docdown.Text.Bib;
using Docdown.Model;
using Docdown.Util;
using Docdown.ViewModel.Commands;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using PandocMark.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Docdown.ViewModel.Editing
{
    public class BibEditorViewModel : EditorViewModel
    {
        public override IFoldingStrategy FoldingStrategy => folding;

        private readonly BibFoldingStrategy folding;
        private readonly BibHighlightColorizer colorizer;
        private readonly IssueBackgroundRenderer issueRenderer;
        private readonly List<Issue> issues;
        private readonly List<BibEntry> entries;

        private readonly MenuItemAction showArticleAction;

        public BibEditorViewModel(WorkspaceItemViewModel item, TextEditor editor) : base(item, editor)
        {
            folding = new BibFoldingStrategy();
            entries = new List<BibEntry>();
            var bibTheme = Theme.Get("Bib");
            colorizer = new BibHighlightColorizer(bibTheme);
            issueRenderer = new IssueBackgroundRenderer(bibTheme);
            LineTransformers.Add(colorizer);
            BackgroundRenderers.Add(issueRenderer);
            issueRenderer.Issues = issues = new List<Issue>();
            showArticleAction = new MenuItemAction("Show Article", null, new ActionCommand(OpenPreview));

            editor.ContextMenuOpening += ContextMenuOpening;
        }

        private void ContextMenuOpening(object sender, System.EventArgs e)
        {
            if (CanShowArticleAtIndex())
            {
                if (!ContextMenuActions.Contains(showArticleAction))
                {
                    ContextMenuActions.Insert(0, showArticleAction);
                }
            }
            else
            {
                ContextMenuActions.Remove(showArticleAction);
            }
        }

        public override void Update()
        {

        }

        public bool CanShowArticleAtIndex()
        {
            var index = TextEditor.MouseIndex();
            if (index >= 0)
            {
                var selected = entries.FirstOrDefault(e => index >= e.SourcePosition && e.SourcePosition + e.SourceLength > index);
                return selected != null && (selected["doi"] != null || selected["isbn"] != null || selected["url"] != null);
            }
            return false;
        }

        public override object FindHoverContent(int index)
        {
            foreach (var issue in issues)
            {
                if (issue.Offset <= index && issue.Offset + issue.Length >= index)
                {
                    return issue.Tooltip;
                }
            }
            return null;
        }

        public override bool FillCompletionList(CompletionList completionList, int selectionStart)
        {
            return false;
        }

        [ChangeListener(nameof(Text))]
        private void TextChanged()
        {
            folding.Entries.Clear();
            issues.Clear();
            colorizer.Items.Clear();
            entries.Clear();
            var t = Text;
            if (t != null)
            {
                entries.AddRange(BibParser.Parse(t, out var errors));
                if (errors.Any())
                {
                    ErrorIndex = errors.Min(e => e.SourcePosition);
                    issues.AddRange(errors.Select(e => TransformError(e)));
                }
                else
                {
                    ErrorIndex = -1;
                }
                colorizer.Items.AddRange(entries);
                folding.Entries.AddRange(entries);
                TextEditor.TextArea.TextView.Redraw();
            }
        }

        private void OpenPreview()
        {
            int index = TextEditor.SelectionStart;
            var selected = entries.FirstOrDefault(e => e.SourcePosition + e.SourceLength > index);
            if (selected == null && entries.Count > 0)
            {
                selected = entries[entries.Count - 1];
            }
            if (selected == null)
            {
                return;
            }

            Task.Run(async () =>
            {
                var result = await ShowPreview(selected);
                if (!result)
                {
                    // Show message box
                }
            });
        }

        private async Task<bool> ShowPreview(BibEntry entry)
        {
            var url = await BibliographyUtility.FindArticle(entry);

            if (url == null)
            {
                return false;
            }
            var name = entry.Title;
            if (name == null)
            {
                name = "Unknown entry";
            }
            const int cutoff = 40;
            if (name.Length > cutoff)
            {
                name = name.Substring(0, cutoff - 3).Trim() + "...";
            }
            await Dispatcher.InvokeAsync(() =>
            {
                var item = new PreviewWorkspaceItem
                {
                    Url = url,
                    Name = name
                };
                var vm = new WorkspaceItemViewModel(Item.Workspace, null, item);
                Item.Workspace.SelectedItem = vm;
            });
            
            return true;
        }

        private Issue TransformError(BibParseError error)
        {
            return new Issue(IssueType.Error, error.SourcePosition, error.SourceLength, error.LineCount, error.Message);
        }
    }
}
