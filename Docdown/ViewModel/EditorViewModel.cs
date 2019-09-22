using Docdown.Editor.Markdown;
using Docdown.Model;
using Docdown.Util;
using Docdown.ViewModel.Commands;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using PandocMark.Syntax;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

using static Docdown.Editor.Markdown.AbstractSyntaxTree;

namespace Docdown.ViewModel
{
    public class EditorViewModel : ObservableObject
    {
        public string Text
        {
            get => text;
            set
            {
                ast = null;
                if (text != null)
                {
                    Item.HasChanged = true;
                }
                Set(ref text, value);
            }
        }

        [ChangeListener(nameof(Text))]
        public Block AbstractSyntaxTree
        {
            get
            {
                if (ast == null && text != null)
                {
                    ast = GenerateAbstractSyntaxTree(text);
                }
                return ast;
            }
        }

        public WorkspaceItemViewModel Item { get; }
        public TextEditor TextEditor { get; }

        public ICommand UpdateCommand => new ActionCommand(Update);

        private string text;
        private Block ast;

        public EditorViewModel(WorkspaceItemViewModel item, TextEditor editor)
        {
            Item = item ?? throw new ArgumentNullException(nameof(item));
            TextEditor = editor ?? throw new ArgumentNullException(nameof(editor));
        }

        public object FindHoverContent(int index)
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

        public void Update()
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

        private Action<int> JumpToLocation()
        {
            return (index) => TextEditor.ScrollTo(index);
        }

        public bool FillCompletionList(CompletionList completionList, int selectionStart)
        {
            return MarkdownCompletionData.FromAST(Text, selectionStart, AbstractSyntaxTree, completionList);
        }
    }
}
