using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Docdown.Text.Bib;
using Docdown.Util;
using HtmlAgilityPack;

namespace Docdown.Bibliography
{
    public static class SciHubLoader
    {
        public static string CurrentSciHubAddress { get; private set; }

        private const string WhereIsSciHubNow = "https://whereisscihub.now.sh/";
        
        public static async Task<string> FindAddress()
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

        public static async Task<string> FindArticle(BibEntry entry)
        {
            var doi = entry["doi"];
            var isbn = entry["isbn"];
            var url = entry["url"];

            if (doi == null && isbn != null)
            {
                doi = await IdentifierUtility.DoiFromIsbn(isbn);
            }

            if (doi != null)
            {
                return await FindArticle(doi);
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

        public static async Task<string> FindArticle(string doi)
        {
            var scihub = await FindAddress();
            var text = await WebUtility.SimpleTextRequest(WebUtility.BuildUrl(scihub, doi));
            var doc = new HtmlDocument();
            doc.LoadHtml(text);
            var iframe = doc.GetElementbyId("pdf");
            if (iframe != null)
            {
                var src = iframe.GetAttributeValue("src", string.Empty);
                if (src.StartsWith("//"))
                {
                    src = "https:" + src;
                }
                return src;
            }
            return null;
        }

        public static async Task<byte[]> LoadArticleByDoi(string doi, ICaptchaSolvingStrategy strategy)
        {
            var url = await FindArticle(doi);
            return await LoadArticle(url, strategy);
        }

        public static async Task<byte[]> LoadArticle(string url, ICaptchaSolvingStrategy strategy)
        {
            var bytes = await WebUtility.SimpleByteRequest(url);
            var data = new SciHubData();
            if (bytes.Length > 6)
            {
                var html = Encoding.UTF8.GetString(bytes, 0, 6);
                if (html == "<html>")
                {
                    var fullHtml = Encoding.UTF8.GetString(bytes);
                    data.Url = url;
                    data.Html = fullHtml;
                    await LoadImage(data);
                    if (data.ImageData != null)
                    {
                        var solved = await strategy.Solve(data.ImageData);
                        await PostCaptcha(data, solved);
                        return data.ArticleData;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return bytes;
                }
            }
            else
            {
                return Array.Empty<byte>();
            }
        }

        public static async Task LoadImage(SciHubData data)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(data.Html);

            data.Id = doc.DocumentNode
                .SelectSingleNode(".//input[@type='hidden']")
                .GetAttributeValue("value", string.Empty);
            var img = doc.DocumentNode
                .SelectSingleNode(".//img")
                .GetAttributeValue("src", string.Empty);
            var shortUrl = data.Url;
            shortUrl = shortUrl.Substring(0, shortUrl.IndexOf('/', 10));
            shortUrl += img;

            data.ImageData = await WebUtility.SimpleByteRequest(shortUrl);
        }

        public static async Task PostCaptcha(SciHubData data, string solved)
        {
            var body = $"id={data.Id}&answer={solved}";
            using (var res = await WebUtility.SimpleRequest(data.Url, HttpMethod.Post, body, CancellationToken.None))
            {
                data.ArticleData = await res.Content.ReadAsByteArrayAsync();
            }
        }
    }
}
