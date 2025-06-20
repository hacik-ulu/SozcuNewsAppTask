using NewsApp.Api.Models;
using NewsApp.Api.Services.Abstract;
using System.Net;

namespace NewsApp.Api.Services.Concrete
{
    public class NewsAppService : INewsAppService
    {
        public async Task<List<NewsAppDto>> GetNewsListAsync()
        {
            var list = new List<NewsAppDto>();
            var url = "https://www.sozcu.com.tr/";
            using var http = new HttpClient();

            // ScrapingBee yaklaşımı: HTML’i indir
            var html = await http.GetStringAsync(url);
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);

            // NYTimes örneğinden XPath ile haber konteynerlerini seç
            var nodes = doc.DocumentNode.SelectNodes("//div[contains(@class,'news-item')]");

            if (nodes == null) return list;

            // ESPN tekniğiyle her nodo döngüye al
            foreach (var node in nodes.Take(10))
            {
                var a = node.SelectSingleNode(".//a");
                if (a == null) continue;

                var href = a.GetAttributeValue("href", "").Trim();
                var fullUrl = href.StartsWith("http") ? href : "https://www.sozcu.com.tr" + href;
                var title = WebUtility.HtmlDecode(a.InnerText.Trim());

                // Detay sayfası çekimi
                var detailHtml = await http.GetStringAsync(fullUrl);
                var detailDoc = new HtmlAgilityPack.HtmlDocument();
                detailDoc.LoadHtml(detailHtml);

                // İçerik (HackerNews örneğine benzer şekilde parse edilir)
                var contentNode = detailDoc.DocumentNode.SelectSingleNode("//div[contains(@class,'content-detail')]");
                var content = contentNode != null ? WebUtility.HtmlDecode(contentNode.InnerText.Trim()) : "";

                // Özet: meta description okuyalım (Multi-source)
                var summary = detailDoc.DocumentNode
                    .SelectSingleNode("//meta[@name='description']")
                    ?.GetAttributeValue("content", "") ?? "";

                // Author ve category
                var author = detailDoc.DocumentNode.SelectSingleNode("//span[contains(@class,'content-author')]")
                    ?.InnerText.Trim() ?? "";
                var category = detailDoc.DocumentNode.SelectSingleNode("//meta[@property='article:section']")
                    ?.GetAttributeValue("content", "") ?? "";

                // Yayın tarihi
                var dateStr = detailDoc.DocumentNode
                    .SelectSingleNode("//meta[@property='article:published_time']")
                    ?.GetAttributeValue("content", "") ?? "";
                var date = DateTime.TryParse(dateStr, out var dt) ? dt : DateTime.Now;

                // Görsel: Open Graph meta
                var imageUrl = detailDoc.DocumentNode
                    .SelectSingleNode("//meta[@property='og:image']")
                    ?.GetAttributeValue("content", "") ?? "";

                // Oluştur ve ekle
                list.Add(new NewsAppDto
                {
                    NewsUrl = fullUrl,
                    ImageUrl = imageUrl,
                    Title = title,
                    Content = content,
                    Summary = summary,
                    Author = author,
                    Category = category,
                    Date = date,
                });
            }

            return list;
        }

    }
}
