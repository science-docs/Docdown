using Docdown.Text.Bib;
using HtmlAgilityPack;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Docdown.Util
{
    public static class BibliographyUtility
    {
        private static readonly Regex URLRegex = new Regex(@"^https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)$", RegexOptions.Compiled);
        private static readonly Regex DOIRegex = new Regex(@"^10\.[a-zA-Z0-9\.]+\/[a-zA-Z0-9\.]+$", RegexOptions.Compiled);
        private static readonly Regex ISBNRegex = new Regex(@"^([0-9]{10}|[0-9]{13})$", RegexOptions.Compiled);

        public static Task<string> SearchBibliographyEntry(string text)
        {
            if (URLRegex.IsMatch(text))
            {
                return SearchUrl(text);
            }
            else if (DOIRegex.IsMatch(text))
            {
                return SearchDOI(text);
            }
            else if (ISBNRegex.IsMatch(text))
            {
                return SearchISBN(text);
            }
            else
            {
                throw new ArgumentException($"Input '{text}' is neither an URL, DOI nor ISBN.");
            }
        }

        public static async Task<string> SearchUrl(string url)
        {
            var entry = new BibEntry()
            {
                Type = "online"
            };

            var html = await WebUtility.SimpleTextRequest(url);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var title = doc.DocumentNode.SelectSingleNode(".//title").InnerText;
            entry.Title = title;

            return entry.ToString();
        }

        public static async Task<string> SearchDOI(string doi)
        {
            string url = $"http://api.crossref.org/works/{doi}/transform/application/x-bibtex";
            try
            {
                var text = await WebUtility.SimpleTextRequest(url);
                return text;
            }
            catch
            {
                return null;
            }
        }

        public static async Task<string> SearchISBN(string isbn)
        {
            string url = $"http://api.crossref.org/works?filter=isbn:{isbn}";
            try
            {
                var json = await WebUtility.SimpleJsonRequest(url);
                var doi = (string)json.SelectToken("message.items[0].DOI");
                return await SearchDOI(doi);
            }
            catch
            {
                return null;
            }
        }
    }
}
