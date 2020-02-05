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
        private static readonly Regex DoiRegex = new Regex(@"^10\.[a-zA-Z0-9\.]+\/[a-zA-Z0-9\.]+$", RegexOptions.Compiled);
        private static readonly Regex IsbnRegex = new Regex(@"^([0-9]{10}|[0-9]{13})$", RegexOptions.Compiled);

        private const string WhereIsSciHubNow = "https://whereisscihub.now.sh/";
        private static string CurrentSciHubAddress;

        public static async Task<string> FindSciHub()
        {
            if (CurrentSciHubAddress == null)
            {
                string html = await WebUtility.SimpleTextRequest(WhereIsSciHubNow);
                var doc = new HtmlDocument();
                doc.LoadHtml(html);
                CurrentSciHubAddress = doc.DocumentNode.SelectSingleNode(".//a").GetAttributeValue("href", string.Empty);
            }
            return CurrentSciHubAddress;
        }

        public static async Task<string> FindSciHubArticle(string doi)
        {
            var scihub = await FindSciHub();
            var text = await WebUtility.SimpleTextRequest(WebUtility.BuildUrl(scihub, doi));
            var doc = new HtmlDocument();
            doc.LoadHtml(text);
            var iframe = doc.GetElementbyId("pdf");
            if (iframe != null)
            {
                var src = iframe.GetAttributeValue("src", string.Empty);
                return src;
            }
            return null;
        }

        public static async Task<string> FindArticle(BibEntry entry)
        {
            var doi = entry["doi"];
            var isbn = entry["isbn"];
            var url = entry["url"];

            if (doi == null && isbn != null)
            {
                doi = await DoiFromIsbn(isbn);
            }

            if (doi != null)
            {
                return await FindSciHubArticle(doi);
            }

            if (url != null)
            {
                if (url.Contains("arxiv.org"))
                {
                    url = url.Replace("/abs/", "/pdf/");
                    return url;
                }
            }

            return null;
        }

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
