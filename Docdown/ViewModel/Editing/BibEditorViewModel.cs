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
using Docdown.Bibliography;

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

        private MenuItemAction showArticleAction;

        public BibEditorViewModel(WorkspaceItemViewModel item) : base(item)
        {
            folding = new BibFoldingStrategy();
            entries = new List<BibEntry>();
            var bibTheme = ThemePersistance.Load("Bib");
            colorizer = new BibHighlightColorizer(bibTheme);
            issueRenderer = new IssueBackgroundRenderer(bibTheme)
            {
                Issues = issues = new List<Issue>()
            };
        }

        private void ContextMenuOpening(object sender, System.EventArgs e)
        {
            if (sender is TextEditor editor && CanShowArticleAtIndex(editor))
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

        public override void Configure(TextEditor editor)
        {
            base.Configure(editor);

            showArticleAction = new MenuItemAction("Show Article", null, new ActionCommand(() => OpenPreview(editor)));

            editor.TextArea.TextView.BackgroundRenderers.Add(issueRenderer);
            editor.TextArea.TextView.LineTransformers.Add(colorizer);
            
            editor.ContextMenuOpening += ContextMenuOpening;
        }

        public override void Update()
        {

        }

        public bool CanShowArticleAtIndex(TextEditor editor)
        {
            var index = editor.MouseIndex();
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
            }
        }

        private void OpenPreview(TextEditor editor)
        {
            int index = editor.SelectionStart;
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
            byte[] article;
            try
            {
                var url = await SciHubLoader.FindArticle(entry);

                if (url == null)
                    return false;

                article = await SciHubLoader.LoadArticle(url, new UICaptchaSolvingStrategy());

                if (article == null)
                    return false;
            }
            catch (SciHubException e)
            {
                var text = Language.Current.Get(e.MessageId);
                Messages.Error(text);
                return false;
            }

            var fs = Item.Workspace.Data.FileSystem;
            var temp = IOUtility.GetTempFile(fs.Directory);
            fs.File.WriteAllBytes(temp, article);

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
                    FullName = temp,
                    Name = name,
                    Type = WorkspaceItemType.Pdf
                };
                var vm = new WorkspaceItemViewModel(Item.Workspace, null, item)
                {
                    PdfPath = temp
                };
                Item.Workspace.SelectedPreview = vm;
            });
            
            return true;
        }

        private Issue TransformError(BibParseError error)
        {
            return new Issue(IssueType.Error, error.SourcePosition, error.SourceLength, error.LineCount, error.Message);
        }
    }
}
