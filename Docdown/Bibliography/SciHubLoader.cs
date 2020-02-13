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
                string html;
                try
                {
                    html = await WebUtility.SimpleTextRequest(WhereIsSciHubNow);
                }
                catch (Exception e)
                {
                    throw new SciHubException(SciHubExceptionType.Address, e);
                }
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

            throw new SciHubException(SciHubExceptionType.Article);
        }

        public static async Task<string> FindArticle(string doi)
        {
            var scihub = await FindAddress();
            string text;
            try
            {
                text = await WebUtility.SimpleTextRequest(WebUtility.BuildUrl(scihub, doi));
            }
            catch (Exception e)
            {
                throw new SciHubException(SciHubExceptionType.Availability, e);
            }
            
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
            throw new SciHubException(SciHubExceptionType.Article);
        }

        public static async Task<byte[]> LoadArticle(string url, ICaptchaSolvingStrategy strategy)
        {
            var data = new SciHubData
            {
                Url = url
            };
            try
            {
                await strategy.Initialize();
                var result = await LoadArticleInternal(data, strategy);
                return result;
            }
            catch
            {
                throw;
            }
            finally
            {
                await strategy.Finish();
            }
        }

        private static async Task<byte[]> LoadArticleInternal(SciHubData data, ICaptchaSolvingStrategy solvingStrategy)
        {
            var bytes = await WebUtility.SimpleByteRequest(data.Url);

            if (bytes == null || bytes.Length < 6)
            {
                throw new SciHubException(SciHubExceptionType.Article);
            }

            if (!IsHtml(bytes))
                return bytes;

            var fullHtml = Encoding.UTF8.GetString(bytes);
            data.Html = fullHtml;

            await LoadImage(data);

            do
            {
                var solved = await solvingStrategy.Solve(data.ImageData);

                if (solved.Aborted)
                    return null;
                if (solved.Reload)
                    return await LoadArticleInternal(data, solvingStrategy);

                await PostCaptcha(data, solved.Captcha);

                if (data.ArticleData == null)
                {
                    await solvingStrategy.Invalidate(solved.Captcha);
                }
            }
            while (data.ArticleData == null);

            return data.ArticleData;
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

            try
            {
                data.ImageData = await WebUtility.SimpleByteRequest(shortUrl);

                if (data.ImageData == null)
                    throw new Exception("Somehow no image data was loaded");
            }
            catch (Exception e)
            {
                throw new SciHubException(SciHubExceptionType.Captcha, e);
            }
        }

        public static async Task PostCaptcha(SciHubData data, string solved)
        {
            var body = $"id={data.Id}&answer={solved}";
            using (var res = await WebUtility.SimpleRequest(data.Url, HttpMethod.Post, body, CancellationToken.None))
            {
                var bytes = await res.Content.ReadAsByteArrayAsync();
                if (!IsHtml(bytes))
                {
                    data.ArticleData = bytes;
                }
            }
        }

        private static bool IsHtml(byte[] bytes)
        {
            if (bytes == null || bytes.Length < 6)
                return false;

            var html = Encoding.UTF8.GetString(bytes, 0, 6);
            return html == "<html>";
        }
    }
}
