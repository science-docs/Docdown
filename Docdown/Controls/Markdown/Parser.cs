//using Bookbinder.Compendium;
//using HtmlAgilityPack;
//using Markdig;
//using System;
//using System.Linq;

//namespace Docdown.Controls.Markdown
//{
//    public class Parser
//    {
//        public static MarkdownPipeline Pipeline { get; }
//            = new MarkdownPipelineBuilder()
//                .UseAdvancedExtensions().Build();

//        public void Parse(BookElement parent, string markdown)
//        {
//            if (parent is null)
//                throw new ArgumentNullException();

//            string html = Markdig.Markdown.ToHtml(markdown, Pipeline);
//            HtmlDocument document = new HtmlDocument();
//            document.LoadHtml(html);
//            Parse(parent, document);
//        }

//        public void Parse(BookElement parent, HtmlDocument markdownDocument)
//        {
//            if (parent is null)
//                throw new ArgumentNullException();

//            parent.Children.Clear();
//            var docNode = markdownDocument.DocumentNode;
//            foreach (var item in docNode.ChildNodes.Select(e => ConvertHtmlNodeToItem(parent, e)))
//            {
//                parent.Children.Add(item);
//            }
//        }

//        private ContentItem ConvertHtmlNodeToItem(BookElement parent, HtmlNode node)
//        {
//            throw new NotImplementedException();
//        }

//        private ContentItem ConvertHtmlNodeToSubItem(HtmlNode node)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}