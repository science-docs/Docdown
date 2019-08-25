using PandocMark.Syntax;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;

namespace Docdown.Editor.Markdown
{
    public class BlockBackgroundRenderer : IBackgroundRenderer
    {
        private readonly BlockTag[] _blockTags =
        {
            BlockTag.FencedCode,
            BlockTag.IndentedCode,
            BlockTag.AtxHeading,
            BlockTag.SetextHeading,
            BlockTag.HtmlBlock,
            BlockTag.BlockQuote
        };

        private readonly Dictionary<BlockTag, Brush> _brushes = new Dictionary<BlockTag, Brush>();
        private Block _abstractSyntaxTree;

        public KnownLayer Layer => KnownLayer.Background;

        public void Draw(TextView textView, DrawingContext drawingContext)
        {
            var ast = _abstractSyntaxTree;
            if (ast is null) return;

            foreach (var line in textView.VisualLines)
            {
                var rc = BackgroundGeometryBuilder.GetRectsFromVisualSegment(textView, line, 0, 1000).FirstOrDefault();
                if (rc == default) continue;

                foreach (var block in AbstractSyntaxTree.EnumerateBlocks(ast.FirstChild))
                {
                    if (block.SourcePosition + block.SourceLength <= line.StartOffset) continue;
                    if (block.SourcePosition > line.LastDocumentLine.EndOffset) break;

                    // Indented code does not terminate (at least in the AST) until a new
                    // block is encountered. Thus blank lines at the end of the block are
                    // highlighted. It looks much better if they're not highlighted.
                    if (block.Tag == BlockTag.IndentedCode)
                    {
                        var length = block.SourcePosition + block.SourceLength - line.StartOffset;
                        if (line.StartOffset + length > textView.Document.TextLength) break;
                        var remainder = textView.Document.GetText(line.StartOffset, length);
                        if (string.IsNullOrWhiteSpace(remainder)) break;
                    }

                    if (_blockTags.Any(tag => tag == block.Tag))
                    {
                        if (_brushes.TryGetValue(block.Tag, out Brush brush) && brush != null)
                        {
                            drawingContext.DrawRectangle(brush, null,
                                new Rect(0, rc.Top, textView.ActualWidth, line.Height));
                        }
                    }
                }
            }

            var segments = new TextSegmentCollection<IssueMarker>();
            foreach (var issue in ast.Document.Issues)
            {
                segments.Add(new IssueMarker(issue));
            }

            var visualLines = textView.VisualLines;
            if (visualLines.Count == 0)
                return;
            int viewStart = visualLines.First().FirstDocumentLine.Offset;
            int viewEnd = visualLines.Last().LastDocumentLine.EndOffset;
            foreach (IssueMarker marker in segments.FindOverlappingSegments(viewStart, viewEnd - viewStart))
            {
                var brush = marker.Brush;
                Pen usedPen = new Pen(brush, 1);
                usedPen.Freeze();
                if (brush == null)
                {
                    continue;
                }

                foreach (Rect r in BackgroundGeometryBuilder.GetRectsForSegment(textView, marker))
                {
                    Point startPoint = r.BottomLeft;
                    Point endPoint = r.BottomRight;

                    double offset = 2.5;

                    int count = Math.Max((int)((endPoint.X - startPoint.X) / offset) + 1, 4);

                    StreamGeometry geometry = new StreamGeometry();

                    using (StreamGeometryContext ctx = geometry.Open())
                    {
                        ctx.BeginFigure(startPoint, false, false);
                        ctx.PolyLineTo(CreatePoints(startPoint, endPoint, offset, count).ToArray(), true, false);
                    }

                    geometry.Freeze();

                    
                    drawingContext.DrawGeometry(Brushes.Transparent, usedPen, geometry);
                }
            }
        }

        IEnumerable<Point> CreatePoints(Point start, Point end, double offset, int count)
        {
            for (int i = 0; i < count; i++)
                yield return new Point(start.X + i * offset, start.Y - ((i + 1) % 2 == 0 ? offset : 0));
        }

        public void UpdateAbstractSyntaxTree(Block ast) { _abstractSyntaxTree = ast; }

        public void OnThemeChanged(Theme theme)
        {
            _brushes.Clear();
            var codeBrush = ColorBrush(theme.HighlightBlockCode.Background);
            _brushes[BlockTag.FencedCode] = codeBrush;
            _brushes[BlockTag.IndentedCode] = codeBrush;
            _brushes[BlockTag.HtmlBlock] = codeBrush;

            var headingBrush = ColorBrush(theme.HighlightHeading.Background);
            _brushes[BlockTag.AtxHeading] = headingBrush;
            _brushes[BlockTag.SetextHeading] = headingBrush;

            _brushes[BlockTag.BlockQuote] = ColorBrush(theme.HighlightBlockQuote.Background);
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

        private class IssueMarker : TextSegment
        {
            public Brush Brush
            {
                get
                {
                    switch (issue.Type)
                    {
                        case IssueType.Info:
                            return Brushes.Blue;
                        case IssueType.Warning:
                            return Brushes.Yellow;
                        case IssueType.Error:
                            return Brushes.Red;
                        default:
                            return null;
                    }
                }
            }

            private Issue issue;

            public IssueMarker(Issue issue)
            {
                this.issue = issue;
                StartOffset = issue.Offset;
                Length = issue.Length;
                EndOffset = issue.Offset + issue.Length;
            }
        }
    }
}
