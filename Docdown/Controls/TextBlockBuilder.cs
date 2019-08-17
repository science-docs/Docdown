using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Docdown.Controls
{
    public class TextBlockBuilder
    {
        private TextBlock block;
        private Span span;

        public TextBlockBuilder()
        {
            Reset();
        }

        public TextBlockBuilder Reset()
        {
            block = new TextBlock()
            {
                MaxWidth = 600,
                TextWrapping = TextWrapping.Wrap
            };
            span = new Span();
            block.Inlines.Add(span);
            return this;
        }

        public TextBlockBuilder Image(ImageSource image)
        {
            return Image(image, 16, 16);
        }

        public TextBlockBuilder Image(ImageSource image, int width, int height)
        {
            var img = new Image
            {
                Width = width,
                Height = height
            };
            img.Source = image;
            span.Inlines.Add(img);
            return this;
        }

        public TextBlockBuilder Text(string text)
        {
            return Text(text, null);
        }

        public TextBlockBuilder Text(string text, Brush foreground)
        {
            var run = new Run(text);
            span.Inlines.Add(run);
            if (foreground != null)
            {
                run.Foreground = foreground;;
            }
            return this;
        }

        public TextBlockBuilder LineBreak()
        {
            span.Inlines.Add(new LineBreak());
            return this;
        }

        public TextBlock Build()
        {
            return block;
        }
    }
}
