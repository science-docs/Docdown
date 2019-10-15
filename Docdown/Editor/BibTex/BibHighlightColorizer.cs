using BibTeXLibrary;
using Docdown.Editor.Markdown;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using PandocMark.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Docdown.Editor.BibTex
{
    public class BibHighlightColorizer : DocumentColorizingTransformer
    {
        public List<BibEntry> Items { get; } = new List<BibEntry>();

        public Theme Theme { get; set; }

        public BibHighlightColorizer(Theme theme)
        {
            Theme = theme;
        }

        protected override void ColorizeLine(DocumentLine line)
        {
            if (Theme is null) return;

            var start = line.Offset;
            var end = line.EndOffset;
            var leadingSpaces = CurrentContext.GetText(start, end - start).Text.TakeWhile(char.IsWhiteSpace).Count();

            foreach (var item in Items.SelectMany(e => e.Items)
                .Where(e => e.SourcePosition + e.SourceLength > start)
                .TakeWhile(e => e.SourcePosition < end))
            {
                var tag = GetHighlight(item.Type);
                if (AbstractSyntaxTree.InlineHighlighter.TryGetValue(tag, out var highlighter))
                {
                    ApplyLinePart(highlighter(Theme), item.SourcePosition, item.SourceLength, start, end, leadingSpaces, 1.0);
                }
            }
        }


        private InlineTag GetHighlight(EntryItemType type)
        {
            switch (type)
            {
                case EntryItemType.Type:
                    return InlineTag.Tex;
                case EntryItemType.Key:
                    return InlineTag.Code;
                case EntryItemType.Name:
                    return InlineTag.Emphasis;
                case EntryItemType.Value:
                    return InlineTag.String;
            }
            return InlineTag.String;
        }

        private void ApplyLinePart(Highlight highlight, int sourceStart, int sourceLength, int lineStart, int lineEnd,
            int leadingSpaces, double magnify)
        {
            var start = Math.Max(sourceStart, lineStart + leadingSpaces);
            var end = Math.Min(lineEnd, sourceStart + sourceLength);
            if (start < end) ChangeLinePart(start, end, element => ApplyHighlight(element, highlight, magnify));
        }

        private static void ApplyHighlight(VisualLineElement element, Highlight highlight, double magnify)
        {
            var trp = element.TextRunProperties;

            var foregroundBrush = ColorBrush(highlight.Foreground);
            if (foregroundBrush != null) trp.SetForegroundBrush(foregroundBrush);

            // Block background highlighting handled by BlockBackgroundRenderer
            // Otherwise selection highlight does not work as expected
            if (!highlight.Name.Contains("Block"))
            {
                var backgroundBrush = ColorBrush(highlight.Background);
                if (backgroundBrush != null) trp.SetBackgroundBrush(backgroundBrush);
            }

            if (!string.IsNullOrWhiteSpace(highlight.FontWeight) || !string.IsNullOrWhiteSpace(highlight.FontStyle))
            {
                var tf = trp.Typeface;
                var weight = ConvertFontWeight(highlight.FontWeight) ?? tf.Weight;
                var style = ConvertFontStyle(highlight.FontStyle) ?? tf.Style;
                var typeFace = new Typeface(tf.FontFamily, style, weight, tf.Stretch);
                trp.SetTypeface(typeFace);
            }

            if (highlight.Underline) trp.SetTextDecorations(TextDecorations.Underline);
            if (double.IsNaN(magnify) == false) trp.SetFontRenderingEmSize(trp.FontRenderingEmSize * magnify);
        }

        private static Brush ColorBrush(string color)
        {
            if (string.IsNullOrWhiteSpace(color)) return null;
            try
            {
                // ReSharper disable once PossibleNullReferenceException
                var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
                brush.Freeze();
                return brush;
            }
            catch (FormatException)
            {
                return null;
            }
            catch (NotSupportedException)
            {
                return null;
            }
        }

        private static FontWeight? ConvertFontWeight(string fontWeight)
        {
            if (string.IsNullOrWhiteSpace(fontWeight)) return null;
            try
            {
                // ReSharper disable once PossibleNullReferenceException
                return (FontWeight)new FontWeightConverter().ConvertFromString(fontWeight);
            }
            catch (FormatException)
            {
                return null;
            }
            catch (NotSupportedException)
            {
                return null;
            }
        }

        private static FontStyle? ConvertFontStyle(string fontStyle)
        {
            if (string.IsNullOrWhiteSpace(fontStyle)) return null;
            try
            {
                // ReSharper disable once PossibleNullReferenceException
                return (FontStyle)new FontStyleConverter().ConvertFromString(fontStyle);
            }
            catch (FormatException)
            {
                return null;
            }
            catch (NotSupportedException)
            {
                return null;
            }
        }

        
    }
}
