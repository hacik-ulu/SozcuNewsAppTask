using NewsApp.Api.Models;
using NewsApp.Api.Services.Abstract;
using System.Net;
using System.Text.RegularExpressions;

namespace NewsApp.Api.Services.Concrete
{
    public class NewsAppService : INewsAppService
    {
        #region Video Src
        //https://www.youtube.com/watch?v=m9zFq6KS94Y&ab_channel=ShaunHalverson buradaki videodan WebScraper, WebScraper2 projesinden yararlandım bu sayfada.
        #endregion

        public async Task<List<NewsAppDto>> GetNewsListAsync()
        {
            // Haber verilerini tutacak dto listesini oluşturdum.
            var newsList = new List<NewsAppDto>();

            // haberlerin çekileceği base url'i verdim.
            //var newsUrl = "https://www.sozcu.com.tr/kategori/ekonomi/";
            var newsUrl = "https://www.sozcu.com.tr/kategori/spor";

            // HttpClient ile bağlantı açtım sornasında newsUrle get isteği attım ve html sayfasının gelen yanıtlarını string olarak tuttum.
            HttpClient httpClient = new HttpClient();
            var response = await httpClient.GetStringAsync(newsUrl);

            // Html içeriğini okumak adına (div,class,span) gelen verileri buraya yükledik - HtmlAgilityPack library indirdim ve kullandım.
            var htmlDocument = new HtmlAgilityPack.HtmlDocument();
            htmlDocument.LoadHtml(response);

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
                // Html içeriğini anlamlı şekilde paralayarak analiz etmeyi ve işlem yapmayı hedefliyoruz.
                var detail = await httpClient.GetStringAsync(totalUrl);
                var detailDocumentary = new HtmlAgilityPack.HtmlDocument();
                detailDocumentary.LoadHtml(detail);

                // Haber başlığını alıp temizliyoruz HtmlDecode ile.
                var newsTitle = WebUtility.HtmlDecode(
                detailDocumentary.DocumentNode.SelectSingleNode("//h1[@class='fw-bold mb-4']")?.InnerText.Trim()
                ?? detailDocumentary.DocumentNode.SelectSingleNode("//h1[@class='author-content-title']")?.InnerText.Trim());

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

#region Example Data From Sozcu
//{
//    "title": "Hırvat dalışçı, rekorlar kitabına girdi",
//        "content": "Hırvat serbest dalışçı Vitomir Maricic, 29 dakika 3 saniyelik zamanlamasıyla su altında en uzun süre nefes tutma rekoru kırarak, 'Guinness Dünya Rekorlar Kitabı'na girmeye hak kazandı. Maricic, rekor denemesini ülkesinin Opatija kentinde yer alan bir otelde 3 metre derinliğindeki havuzda gerçekleştirdi. Deneme, Guinness Dünya Rekorları'nın gerekliliklerine uygunluk sağlanması amacıyla hakemler tarafından resmi olarak takip edildi. Rekor denemesine 100'den fazla izleyici de tanıklık etti. Vitomir Maricic, suyun altında 29 dakika 3 saniye nefesini tutarak dünya rekoruna imza attı ve adını 'Guinness Dünya Rekorlar Kitabı'na yazdırmayı başardı. Bu alanda bir önce rekoru, 2021 yılında 24 dakika 37 saniyelik süresiyle Hırvat sporcu Budimir Sobat kırmıştı.",
//        "summary": "Hırvat serbest dalışçı Vitomir Maricic'den su altında en uzun süre nefes tutma rekoru. Maricic, 29 dakika 3 saniyelik zamanlamasıyla adını \"Guinness Dünya Rekorlar Kitabı\"na yazdırdı",
//        "newsUrl": "https://www.sozcu.com.tr/hirvat-dalisci-rekorlar-kitabina-girdi-p185776",
//        "imageUrl": "https://sozcu01.sozcucdn.com/sozcu/production/uploads/images/2025/6/ekran-goruntusu-20250619-191305png-OIpwAlQsR0_HLKkczd0emA.png?w=1270&h=675&mode=crop&scale=both",
//        "category": "Diğer Sporlar",
//        "author": "AA",
//        "date": "2025-06-21T13:13:19.6519226+03:00"
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

