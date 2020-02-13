using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Docdown.Text.Bib;
using Docdown.Util;
using HtmlAgilityPack;

namespace Docdown.Bibliography
{
    public static class IdentifierUtility
    {
        private static readonly Regex URLRegex = new Regex(@"^https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)$", RegexOptions.Compiled);
        private static readonly Regex DoiRegex = new Regex(@"^10\.[a-zA-Z0-9\.]+\/[a-zA-Z0-9\.]+$", RegexOptions.Compiled);
        private static readonly Regex IsbnRegex = new Regex(@"^([0-9]{10}|[0-9]{13})$", RegexOptions.Compiled);

        public static bool IsIsbn(string value)
        {
            return IsbnRegex.IsMatch(value);
        }

        public static bool IsDoi(string value)
        {
            return DoiRegex.IsMatch(value);
        }

        public static async Task<string> DoiFromIsbn(string isbn)
        {
            if (isbn is null)
            {
                throw new ArgumentNullException(nameof(isbn));
            }
            isbn = isbn.Replace("-", "");
            if (!IsIsbn(isbn))
            {
                throw new ArgumentException(isbn + " is not a valid ISBN");
            }

            string url = $"http://api.crossref.org/works?filter=isbn:{isbn}";
            try
            {
                var json = await WebUtility.SimpleJsonRequest(url);
                var doi = (string)json.SelectToken("message.items[0].DOI");
                return string.IsNullOrWhiteSpace(doi) ? null : doi;
            }
            catch
            {
                return null;
            }
        }

        public static Task<string> SearchBibliographyEntry(string text)
        {
            if (URLRegex.IsMatch(text))
            {
                return SearchUrl(text);
            }
            else if (IsDoi(text))
            {
                return SearchDoi(text);
            }
            else if (IsIsbn(text))
            {
                return SearchIsbn(text);
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

        public static async Task<string> SearchDoi(string doi)
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

        public static async Task<string> SearchIsbn(string isbn)
        {
            var doi = await DoiFromIsbn(isbn);
            if (doi != null)
            {
                return await SearchDoi(doi);
            }
            return null;
        }
    }
}
