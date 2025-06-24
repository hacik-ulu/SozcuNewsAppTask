using NewsApp.Api.Models;
using NewsApp.Api.Services.Abstract;
using System.Net;
using System.Text.RegularExpressions;

namespace NewsApp.Api.Services.Concrete
{
    public sealed class NewsAppService : INewsAppService
    {
        public async Task<List<NewsAppDto>> GetNewsListAsync()
        {
            // Haber verilerini tutacak dto listesini oluşturdum.
            var newsList = new List<NewsAppDto>();

            // haberlerin çekileceği base url'i verdim.
            var newsUrl = "https://www.sozcu.com.tr/kategori/spor";

            // HttpClient ile bağlantı açtım sornasında newsUrle get isteği attım ve html sayfasının gelen yanıtlarını string olarak tuttum.
            HttpClient httpClient = new HttpClient();
            var response = await httpClient.GetStringAsync(newsUrl);

            // Html içeriğini okumak adına (div,class,span) gelen verileri buraya yükledik - HtmlAgilityPack library indirdim ve kullandım.
            var htmlDocument = new HtmlAgilityPack.HtmlDocument();
            htmlDocument.LoadHtml(response);  // haber sitesinden gelen verileri htmlDocumente yükleyerek okumayı sağladım. 

            // Haber sayfasının incele -MANŞET ALANLARI- kısmından haberlerin nodeunu almak adına classları Xpath olarak verdim
            var nodes = htmlDocument.DocumentNode.SelectNodes("//div[contains(@class, 'row') and contains(@class, 'mb-4')]");
            if (nodes == null)
            {
                return newsList;
            }

            // Eğer haber içeriğini ve node kısmını doğru okuduysam foreachle 20 ornek haberi gezdim.
            foreach (var newsNode in nodes.Take(20))
            {
                // a href bağlantısına bastığımızda haber detaylarına erişebilmek için her birinin linklerini kontrol ettim.
                var anchorLink = newsNode.SelectSingleNode(".//a");
                if (anchorLink == null)
                {
                    // eğer haberin tıklanabilir detay sayfası yoksa diğerini incelemeye geçiyoruz.
                    continue;
                }

                // haberin bağlantı detay adresini alıp boslukları temizledim.
                // ayrıca hrefLink'in tam/doğru bir url oldup olmadığını kontrol ettim yani http ile mi baslıyor yoksa birlestirilmeli mi diye
                var hrefLink = anchorLink.GetAttributeValue("href", "").Trim();
                var totalUrl = hrefLink.StartsWith("http") ? hrefLink : "https://www.sozcu.com.tr" + hrefLink;


                // Haber detay sayfasının Html içeriğini string olarak indiriyoruz.
                // Html içeriğini anlamlı şekilde parcalayarak analiz etmeyi ve işlem yapmayı hedefledim.
                var detail = await httpClient.GetStringAsync(totalUrl);
                var detailDocumentary = new HtmlAgilityPack.HtmlDocument();
                detailDocumentary.LoadHtml(detail);

                // Haber başlığını alıp temizliyoruz HtmlDecode ile - ozel,okunamayan karakterlerin temizlesnmesini hedefledim -.
                var newsTitle = WebUtility.HtmlDecode(
                detailDocumentary.DocumentNode.SelectSingleNode("//h1[@class='fw-bold mb-4']")?.InnerText.Trim()
                ?? detailDocumentary.DocumentNode.SelectSingleNode("//h1[@class='author-content-title']")?.InnerText.Trim());
                // null gelirse yedek olarak h1 class author-content-title'den kontrol sağlıyoruz. bazı haberlerde title fw bold mb 4 ile sağlanırken bazılarında author content title ile sağlaniyor.


                // Haberin içeriğinin classını vererek node'unu buluyoruz.
                var newsContentNode = detailDocumentary.DocumentNode
                    .SelectSingleNode("//div[contains(@class,'article-body')]");

                // Haber içeriğindeki özel karakterleri, boşlukları vs temizliyoruz.
                var newsContent = newsContentNode != null
                 ? Regex.Replace(
                     WebUtility.HtmlDecode(newsContentNode.InnerText)
                         .Replace("\\", "")
                         .Replace("\"", "'")
                         .Replace("\r", " ")
                         .Replace("\n", " ")
                         .Replace("\t", " ")
                         .Replace("\u200b", " ")
                         .Replace("&nbsp;", " ")
                         .Trim(),
                     @"\s+", " ")
                 : "";


                // Haber özetinin classını geçiyoruz.
                // <meta> etiketleri genelde haberin özetini (description olarak burada)ve görselleri daha güvenilir olarak tutuyor, veri cekerken de daha temiz sonuc elde ettim.

                var newsSummary = detailDocumentary.DocumentNode
                    .SelectSingleNode("//meta[@name='description']")
                    ?.GetAttributeValue("content", "") ?? "";

                var newsAuthor = detailDocumentary.DocumentNode
                    .SelectSingleNode("//span[contains(@class, 'd-flex') and contains(@class, 'align-items-center')]")
                    ?.InnerText.Trim() ?? "";

                var newsCategory = detailDocumentary.DocumentNode
                    .SelectSingleNode("//a[contains(@class, 'small') and contains(@class, 'fw-bold') and contains(@class, 'text-primary')]")
                    ?.InnerText.Trim() ?? "";

                var dateNews = detailDocumentary.DocumentNode
                .SelectSingleNode("//time")
                ?.GetAttributeValue("datetime", "");
                var decodedDate = WebUtility.HtmlDecode(dateNews);
                var datePublish = DateTime.Parse(decodedDate);

                var imageUrl = detailDocumentary.DocumentNode
                    .SelectSingleNode("//meta[@property='og:image']")
                    ?.GetAttributeValue("content", "") ?? "";

                // Tüm bu çektiğimiz verileri en basta olusturduğumuz listeye ekleyerek Dto nesnesine geçiriyoruz.
                // Özel karakterlerin temizlenmesi amacıyla en sonda HtmlDecode ediyoruz.
                newsList.Add(new NewsAppDto
                {
                    NewsUrl = totalUrl,
                    ImageUrl = WebUtility.HtmlDecode(imageUrl),
                    Title = WebUtility.HtmlDecode(newsTitle),
                    Content = WebUtility.HtmlDecode(newsContent),
                    Summary = WebUtility.HtmlDecode(newsSummary),
                    Author = WebUtility.HtmlDecode(newsAuthor),
                    Category = WebUtility.HtmlDecode(newsCategory),
                    Date = datePublish
                });
            }

            return newsList;
        }
    }
}




