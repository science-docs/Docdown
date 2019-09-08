﻿using Docdown.Model;
using HtmlAgilityPack;
using System.Threading.Tasks;

namespace Docdown.Util
{
    public static class BibliographyUtility
    {
        public static Task<string> SearchBibliographyEntry(string text)
        {
            return null;
        }

        public static async Task<string> SearchUrl(string url)
        {
            var entry = new BibliographyEntry()
            {
                Type = "online"

            };

            var html = await WebUtility.SimpleTextRequest(url);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var title = doc.DocumentNode.SelectSingleNode(".//title").InnerText;
            entry.Fields.Add("title", title);

            return entry.ToString();
        }

        public static Task<string> SearchDOI(string doi)
        {
            return null;
        }

        public static Task<string> SearchISBN(string isbn)
        {
            return null;
        }
    }
}