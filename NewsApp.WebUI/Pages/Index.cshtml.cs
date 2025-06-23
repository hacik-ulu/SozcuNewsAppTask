using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsApp.WebUI.Context;
using NewsApp.WebUI.Dto;

namespace NewsApp.WebUI.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly ElasticsearchContext _elasticsearchContext;

        public List<NewsAppDto> News { get; set; } = new(); //News adında boş liste propertysi new'liyoruz.

        public IndexModel(ILogger<IndexModel> logger, ElasticsearchContext elasticsearchContext)
        {
            _logger = logger;
            _elasticsearchContext = elasticsearchContext;
        }

        public async Task OnGetAsync()
        {
            var news = await _elasticsearchContext.Client.SearchAsync<NewsAppDto>(x => x
                .Index("news-app-demo") // Hangi tablodan veri cekceğimizi belirtiyoruz.
                .Query(y => y.MatchAll())
                .Sort(z => z.Descending(t => t.Date)) // en yeni tarihe göre en yeni haber gelecek
                .Size(20)
            );

            //.Documents-> Elasticsearchden gelen ve NewsAppDto nesneleriyle eşleşen koleksiyon.
            //News = news.Documents.ToList(); // Elasticsearchden gelen verileri (dokumanlar) List<NewsAppDto> tipine ceviriyor.

            News = news.Hits.Select(x =>  // Elasticden gelen her bir belge(hit) icin select yapiyoruz ve belgeye x atıyoruz
            {
                var documentary = x.Source;  // Elasticsearchdeki asıl veriyi degiskene atiyoruz
                documentary.Id = x.Id; // Id'yi manuel olarak  DTO'ya ekliyoruz
                return documentary; // id'li documenti art�k geri d�n�yoruz ve listeliyoruz.
            }).ToList();
        }

    }
}


// Sayfalama, haber iceriklerini ve modellerini listeleme, arama çubuğu nest ile elastic search
#region
//https://www.elastic.co/docs/reference/elasticsearch/clients/dotnet/examples

// https://stackoverflow.com/questions/33834141/elasticsearch-and-nest-why-am-i-missing-the-id-field-on-a-query?utm_source=chatgpt.com Hits ile Id yakalama
#endregion