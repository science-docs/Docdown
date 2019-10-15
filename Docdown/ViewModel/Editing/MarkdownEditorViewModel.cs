using Docdown.Editor;
using Docdown.Editor.Markdown;
using Docdown.Model;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using PandocMark.Syntax;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

using static Docdown.Editor.Markdown.AbstractSyntaxTree;

namespace Docdown.ViewModel.Editing
{
    public class MarkdownEditorViewModel : EditorViewModel
    {
        [ChangeListener(nameof(Text))]
        public Block AbstractSyntaxTree { get; private set; }

        public override IFoldingStrategy FoldingStrategy => folding;

        public override char[] CompletionMarkers => new char[] { '^', '@', '\\', '<' };

        private readonly MarkdownFoldingStrategy folding;
        private readonly MarkdownHighlightingColorizer colorizer;
        private readonly BlockBackgroundRenderer blockRenderer;
        private readonly IssueBackgroundRenderer issueRenderer;

        private int _previousLineCount = -1;

        public MarkdownEditorViewModel(WorkspaceItemViewModel item, TextEditor editor) : base(item, editor)
        {
            var theme = Theme.Get("Markdown");
            folding = new MarkdownFoldingStrategy();
            colorizer = new MarkdownHighlightingColorizer(theme);
            LineTransformers.Add(colorizer);
            blockRenderer = new BlockBackgroundRenderer(theme);
            BackgroundRenderers.Add(blockRenderer);
            issueRenderer = new IssueBackgroundRenderer(theme);
            BackgroundRenderers.Add(issueRenderer);

            editor.Document.PropertyChanged += OnEditBoxPropertyChanged;
        }

        public override object FindHoverContent(int index)
        {
            foreach (var issue in AbstractSyntaxTree.Document.Issues)
            {
                if (issue.Offset <= index && issue.Offset + issue.Length >= index)
                {
                    return issue.Tooltip;
                }
            }

            var inline = SpanningInline(AbstractSyntaxTree, index, out var block);
            if (inline != null)
            {
                switch (inline.Tag)
                {
                    case InlineTag.Link:
                        return GetLinkInlineTooltip(AbstractSyntaxTree, inline);
                }
            }
            else if (block != null && block.Tag == BlockTag.Meta)
            {
                var metaDataDesc = FindMetaEntry(block, index);
                if (metaDataDesc != null)
                {
                    return metaDataDesc;
                }
            }

            return null;
        }

        private object FindMetaEntry(Block meta, int index)
        {
            foreach (var entry in meta.MetaData.Entries)
            {
                if (entry.NameStartPosition <= index && entry.ValueStartPosition + entry.ValueLength >= index)
                {
                    var modelEntry = AppViewModel.Instance.Settings.SelectedTemplate.MetaData.Entries.FirstOrDefault(e => e.Name == entry.Name);
                    if (modelEntry != null)
                    {
                        return new MarkdownMetaCompletionData(modelEntry).Description;
                    }
                }
            }
            return null;
        }

        private void OnEditBoxPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "LineCount")
            {
                var currentLineCount = TextEditor.LineCount;
                // -1 means the editor has never had text in it.
                if (_previousLineCount != -1)
                {
                    ListInputHandler.AdjustList(TextEditor, currentLineCount > _previousLineCount);
                }
                _previousLineCount = currentLineCount;
            }
        }

        private object GetLinkInlineTooltip(Block ast, Inline link)
        {
            string text = link.FirstChild.LiteralContent;
            if (ast.Document.ReferenceMap.TryGetValue(text, out var reference))
            {
                char marker = text[0];
                if (marker == '@')
                {
                    return new MarkdownCitationCompletionData(reference).Description;
                }
                else if (marker == '^')
                {
                    return new MarkdownFootnoteCompletionData(reference).Description;
                }
            }
            return null;
        }

        public override void Update()
        {
            try
            {
                var headers = EnumerateHeader(AbstractSyntaxTree);
                var outline = new Outline(headers);
                var wordCount = CountWords(AbstractSyntaxTree, Text);
                Item.WordCount = wordCount;
                Item.Outline = new OutlineViewModel(outline, JumpToLocation());

                foreach (var issue in MarkdownValidator.Validate(AbstractSyntaxTree, Text, Item.Data))
                {
                    AbstractSyntaxTree.Document.Issues.Add(issue);
                }
                if (AbstractSyntaxTree.Document.Issues.Count > 0)
                {
                    TextEditor.TextArea.TextView.Redraw();
                }
                Item.HasValidationErrors = AbstractSyntaxTree.Document.Issues.Any(e => e.Type == IssueType.Error);
            }
            catch
            {
                Trace.WriteLine("Debounced update failed", "Editor");
            }
        }

        public override bool FillCompletionList(CompletionList completionList, int selectionStart)
        {
            return MarkdownCompletionData.FromAST(Text, selectionStart, AbstractSyntaxTree, completionList);
        }

        [ChangeListener(nameof(Text))]
        private void TextChanged()
        {
            if (Text != null)
            {
                AbstractSyntaxTree = GenerateAbstractSyntaxTree(Text);
                folding.AbstractSyntaxTree = AbstractSyntaxTree;
                folding.Text = Text;
                colorizer.UpdateAbstractSyntaxTree(AbstractSyntaxTree);
                blockRenderer.UpdateAbstractSyntaxTree(AbstractSyntaxTree);
                issueRenderer.Issues = AbstractSyntaxTree.Document.Issues;
            }
            else
            {
                AbstractSyntaxTree = null;
            }
        }
    }
}
