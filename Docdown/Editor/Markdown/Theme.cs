﻿using Docdown.ViewModel;
using System;

namespace Docdown.Editor.Markdown
{
    public class ThemeChangedEventArgs : EventArgs
    {
        public Theme Theme { get; set; }
    }

    public class Theme : ObservableObject
    {
        private string _name = "Zenburn";
        private string _editorBackground = "#404040";
        private string _editorForeground = "#ccc";
        private string _spellCheckError = "#f00";

        private Highlight _highlightHeading = new Highlight { Name = "Heading", Foreground = "#CF6A4C" };
        private Highlight _highlightEmphasis = new Highlight { Name = "Emphasis", Foreground = "#8F9D67" };
        private Highlight _highlightStrongEmphasis = new Highlight { Name = "StrongEmphasis", Foreground = "#8F9D67" };
        private Highlight _highlightInlineCode = new Highlight { Name = "InlineCode", Foreground = "#AC884C" };
        private Highlight _highlightBlockCode = new Highlight { Name = "BlockCode", Foreground = "#AC884C" };
        private Highlight _highlightBlockQuote = new Highlight { Name = "BlockQuote", Foreground = "#8F9D67" };
        private Highlight _highlightLink = new Highlight { Name = "Link", Foreground = "#2aa198", Underline = true };
        private Highlight _highlightImage = new Highlight { Name = "Image", Foreground = "#6F8F3F" };
        private Highlight _highlightComment = new Highlight { Name = "Comment", Foreground = "#5F5A60" };
        private Highlight _highlightTex = new Highlight { Name = "Tex", Foreground = "#3786D4" };
        private Highlight _highlightTodo = new Highlight { Name = "Todo", Foreground = "#B8D7A3" };

        public string Name
        {
            get => _name;
            set => Set(ref _name, value);
        }

        public string EditorBackground
        {
            get => _editorBackground;
            set => Set(ref _editorBackground, value);
        }

        public string EditorForeground
        {
            get => _editorForeground;
            set => Set(ref _editorForeground, value);
        }

        public Highlight HighlightHeading
        {
            get => _highlightHeading;
            set => Set(ref _highlightHeading, value);
        }

        public Highlight HighlightEmphasis
        {
            get => _highlightEmphasis;
            set => Set(ref _highlightEmphasis, value);
        }

        public Highlight HighlightStrongEmphasis
        {
            get => _highlightStrongEmphasis;
            set => Set(ref _highlightStrongEmphasis, value);
        }

        public Highlight HighlightInlineCode
        {
            get => _highlightInlineCode;
            set => Set(ref _highlightInlineCode, value);
        }

        public Highlight HighlightBlockCode
        {
            get => _highlightBlockCode;
            set => Set(ref _highlightBlockCode, value);
        }

        public Highlight HighlightBlockQuote
        {
            get => _highlightBlockQuote;
            set => Set(ref _highlightBlockQuote, value);
        }

        public Highlight HighlightLink
        {
            get => _highlightLink;
            set => Set(ref _highlightLink, value);
        }

        public Highlight HighlightImage
        {
            get => _highlightImage;
            set => Set(ref _highlightImage, value);
        }

        public Highlight HighlightComment
        {
            get => _highlightComment;
            set => Set(ref _highlightComment, value);
        }

        public Highlight HighlightTex
        {
            get => _highlightTex;
            set => Set(ref _highlightTex, value);
        }

        public Highlight HighlightTask
        {
            get => _highlightTodo;
            set => Set(ref _highlightTodo, value);
        }

        public string SpellCheckError
        {
            get => _spellCheckError;
            set => Set(ref _spellCheckError, value);
        }
    }
}