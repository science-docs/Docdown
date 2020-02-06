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

namespace Docdown.Editor
{
    public class IssueBackgroundRenderer : IBackgroundRenderer
    {
        public KnownLayer Layer => KnownLayer.Background;

        public IEnumerable<Issue> Issues { get; set; }

        public Theme Theme { get; set; }

        public IssueBackgroundRenderer(Theme theme)
        {
            Theme = theme;
        }

        public void Draw(TextView textView, DrawingContext drawingContext)
        {
            if (Issues == null)
                return;

            var segments = new TextSegmentCollection<IssueMarker>();
            foreach (var issue in Issues)
            {
                segments.Add(new IssueMarker(Theme, issue));
            }

            var visualLines = textView.VisualLines;
            if (visualLines.Count == 0)
                return;
            int viewStart = visualLines.First().FirstDocumentLine.Offset;
            int viewEnd = visualLines.Last().LastDocumentLine.EndOffset;
            foreach (var marker in segments.FindOverlappingSegments(viewStart, viewEnd - viewStart))
            {
                var brush = marker.Brush;
                var usedPen = new Pen(brush, 1);
                usedPen.Freeze();
                if (brush == null)
                {
                    continue;
                }

                foreach (var r in BackgroundGeometryBuilder.GetRectsForSegment(textView, marker))
                {
                    var startPoint = r.BottomLeft;
                    var endPoint = r.BottomRight;

                    double offset = 2.5;

                    int count = Math.Max((int)((endPoint.X - startPoint.X) / offset) + 1, 4);

                    var geometry = new StreamGeometry();

                    using (StreamGeometryContext ctx = geometry.Open())
                    {
                        ctx.BeginFigure(startPoint, false, false);
                        ctx.PolyLineTo(CreatePoints(startPoint, offset, count), true, false);
                    }

                    geometry.Freeze();


                    drawingContext.DrawGeometry(Brushes.Transparent, usedPen, geometry);
                }
            }
        }

        private Point[] CreatePoints(Point start, double offset, int count)
        {
            var points = new Point[count];
            for (int i = 0; i < count; i++)
                points[i] = new Point(start.X + i * offset, start.Y - ((i + 1) % 2 * offset));

            return points;
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
                            return theme.Info ?? Brushes.Blue;
                        case IssueType.Warning:
                            return theme.Warning ?? Brushes.Yellow;
                        case IssueType.Error:
                            return theme.Error ?? Brushes.Red;
                        default:
                            return null;
                    }
                }
            }

            private readonly Issue issue;
            private readonly Theme theme;

            public IssueMarker(Theme theme, Issue issue)
            {
                this.theme = theme;
                this.issue = issue;
                StartOffset = issue.Offset;
                Length = issue.Length;
                EndOffset = issue.Offset + issue.Length;
            }
        }
    }
}
