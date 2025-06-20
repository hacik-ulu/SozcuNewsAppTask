using NewsApp.Api.Models;
using NewsApp.Api.Services.Abstract;
using System.Net;
using System.Text.RegularExpressions;

namespace NewsApp.Api.Services.Concrete
{
    public class NewsAppService : INewsAppService
    {
        public async Task<List<NewsAppDto>> GetNewsListAsync()
        {
            // Haber verilerini tutacak dto listesini oluşturdum.
            var newsList = new List<NewsAppDto>();

            // haberlerin çekileceği base url'i verdim.
            var newsUrl = "https://www.sozcu.com.tr/kategori/ekonomi/";

            // HttpClient ile bağlantı açtım sornasında newsUrle get isteği attım ve html sayfasının gelen yanıtlarını string olarak tuttum.
            HttpClient httpClient = new HttpClient();
            var response = await httpClient.GetStringAsync(newsUrl);

            // Html içeriğini okumak adına (div,class,span) gelen verileri buraya yükledik
            var htmlDocument = new HtmlAgilityPack.HtmlDocument();
            htmlDocument.LoadHtml(response);

            // Haber sayfasının incele -MANŞET ALANLARI- kısmından haberlerin nodeunu almak adına classları Xpath olarak verdim
            var nodes = htmlDocument.DocumentNode.SelectNodes("//div[contains(@class, 'row') and contains(@class, 'mb-4')]");
            if (nodes == null)
            {
                return newsList;
            }

            // Eğer haber içeriğini ve node kısmını doğru okuduysam foreachle 30 ornek haberi gezdim.
            foreach (var newsNode in nodes.Take(30))
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

                // Haber başlığını alıp temizliyoruz.
                var newsTitle = WebUtility.HtmlDecode(anchorLink.InnerText.Trim());

                // Haber detay sayfasının Html içeriğini string olarak indiriyoruz.
                // Html içeriğini anlamlı şekilde paralayarak analiz etmeyi ve işlem yapmayı hedefliyoruz.
                var detail = await httpClient.GetStringAsync(totalUrl);
                var detailDocumentary = new HtmlAgilityPack.HtmlDocument();
                detailDocumentary.LoadHtml(detail);

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
                    .SelectSingleNode("//meta[@property='article:published_time']")
                    ?.GetAttributeValue("content", "") ?? "";
                var datePublish = DateTime.TryParse(dateNews, out var dt) ? dt : DateTime.Now;

                var imageUrl = detailDocumentary.DocumentNode
                    .SelectSingleNode("//meta[@property='og:image']")
                    ?.GetAttributeValue("content", "") ?? "";

                // Tüm bu çektiğimiz verileri en basta olusturduğumuz listeye ekleyerek Dto nesnesine geçiriyoruz.
                newsList.Add(new NewsAppDto
                {
                    NewsUrl = totalUrl,
                    ImageUrl = imageUrl,
                    Title = newsTitle,
                    Content = newsContent,
                    Summary = newsSummary,
                    Author = newsAuthor,
                    Category = newsCategory,
                    Date = datePublish,
                });
            }

            return newsList;
        }
    }
}

#region Example Data From Sozcu
//{
//    "title": "THY İspanyollara ortak oluyor",
//        "content": "Türk Hava Yolları, İspanyol havayolu şirketi Air Europa’da azınlık hissesi satın almak için teklifte bulunmayı değerlendiriyor. Bu konuyla ilgilenen diğer büyük havayolları da bulunuyor.\nReuters’ın Cuma günü yayınladığı habere göre, hisse için bağlayıcı tekliflerin Temmuz ayı başına kadar verilmesi gerekiyor.\nHidalgo ailesinin Globalia holding şirketine ait olan Air Europa, birçok büyük havayolu şirketinin ilgisini çekiyor. Air France KLM ve Lufthansa da İspanyol havayolu şirketinde hisse satın almak için görüşmeler yürütüyor.\nTürk Hava Yolları şu anda Air Europa ile kod paylaşımı anlaşmasına sahip. Air Europa, gelirinin yüzde 25’inden fazlasını Avrupa operasyonlarından elde ediyor.",
//        "summary": "Türk Hava Yolları, Air Europa’da azınlık hissesi satın almak için teklif hazırlığında. Lufthansa ve Air France-KLM gibi devlerle rekabet eden THY, Temmuz başına kadar kararını verecek.",
//        "newsUrl": "https://www.sozcu.com.tr/thy-ispanyollara-ortak-oluyor-p186097",
//        "imageUrl": "https://sozcu01.sozcucdn.com/sozcu/production/uploads/images/2025/4/thy1jpg-Fw1iBDFWR0KFvcwO360erQ.jpg?w=1270&amp;h=675&amp;mode=crop&amp;scale=both",
//        "category": "Ekonomi",
//        "author": "Haber Merkezi",
//        "date": "2025-06-20T17:46:41.6034957+03:00"
//    }
#endregion

#region Sources 

//https://www.youtube.com/watch?v=m9zFq6KS94Y&ab_channel=ShaunHalverson (En cok burası)

//HtmlAgilityPack ve XPath Kullanımı
//Html Agility Pack Resmi GitHub Sayfası
// https://github.com/zzzprojects/html-agility-pack


//W3Schools XPath Tutorial
// https://www.w3schools.com/xml/xpath_syntax.asp

// HttpClient Kullanımı
//HttpClient in .NET (Microsoft Docs)
// https://learn.microsoft.com/en-us/dotnet/api/system.net.http.httpclient

//HttpClient best practices (Stack Overflow)
// https://stackoverflow.com/questions/15705092/do-httpclient-and-httpclienthandler-have-to-be-disposed

// Regex ve Text Temizleme
//Regex.Replace Açıklaması (Microsoft Docs)
// https://learn.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.regex.replace


//Basic Web Scraping with Selenium in C# for Google News
//https://medium.com/%40ertekinozturgut/basic-web-scraping-via-selenium-in-c-for-google-news-c012c2b4939d

//ScrapingBee – “Web Scraping with Html Agility Pack
// https://www.scrapingbee.com/blog/html-agility-pack/   xpath syntaxlara baktım foreachler gibi biçok konuda buradan yardım aldım. önemli

//HTML Agility Pack: How to Parse Data (Tutorial 2025)
//https://www.zenrows.com/blog/html-agility-pack

//Web Scraping in C#: Complete Guide 2025
//https://www.zenrows.com/blog/web-scraping-c-sharp

#endregion

