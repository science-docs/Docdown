using BibTeXLibrary;
using Docdown.Editor;
using Docdown.Editor.BibTex;
using Docdown.Editor.Markdown;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using PandocMark.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace Docdown.ViewModel.Editing
{
    public class BibEditorViewModel : EditorViewModel
    {
        public override IFoldingStrategy FoldingStrategy => folding;

        private readonly BibFoldingStrategy folding;
        private readonly BibHighlightColorizer colorizer;
        private readonly IssueBackgroundRenderer issueRenderer;
        private readonly List<Issue> issues;

        public BibEditorViewModel(WorkspaceItemViewModel item, TextEditor editor) : base(item, editor)
        {
            folding = new BibFoldingStrategy();
            var bibTheme = Theme.Get("Bib");
            colorizer = new BibHighlightColorizer(bibTheme);
            issueRenderer = new IssueBackgroundRenderer(bibTheme);
            LineTransformers.Add(colorizer);
            BackgroundRenderers.Add(issueRenderer);
            issues = new List<Issue>();
            issueRenderer.Issues = issues;
        }

        public override void Update()
        {

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
            var t = Text;
            if (t != null)
            {
                var entries = BibParser.Parse(t, out var errors);
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

        private Issue TransformError(BibParseError error)
        {
            return new Issue(IssueType.Error, error.SourcePosition, error.SourceLength, error.LineCount, error.Message);
        }
    }
}
