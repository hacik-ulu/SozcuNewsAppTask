using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsApp.WebUI.Context;
using NewsApp.WebUI.Dto;

namespace NewsApp.WebUI.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly ElasticsearchContext _elasticsearchContext;

        public List<NewsAppDto> News { get; set; } = new(); //News adýnda boþ liste propertysi new'liyoruz.

        public IndexModel(ILogger<IndexModel> logger, ElasticsearchContext elasticsearchContext)
        {
            _logger = logger;
            _elasticsearchContext = elasticsearchContext;
        }

        public async Task OnGetAsync()
        {
            var news = await _elasticsearchContext.Client.SearchAsync<NewsAppDto>(x => x
                .Index("news-app-demo") // Hangi tablodan veri cekceðimizi belirtiyoruz.
                .Query(y => y.MatchAll())
                .Sort(z => z.Descending(t => t.Date)) // en yeni tarihe göre en yeni haber gelecek
                .Size(20)
            );

            //.Documents-> Elasticsearchden gelen ve NewsAppDto nesneleriyle eþleþen koleksiyon.
            //News = news.Documents.ToList(); // Elasticsearchden gelen verileri (dokümanlarý) List<NewsAppDto> tipine ceviriyor.

            News = news.Hits.Select(x =>  // Elasticden gelen her bir belge(hit) için select yapýyoruz ve belgeye x atýyoruz
            {
                var documentary = x.Source;  // Elasticsearchdeki asýl veriyi deðiþkene atýyoruz
                documentary.Id = x.Id; // Id'yi manuel olarak  DTO'ya ekliyoruz
                return documentary; // id'li documenti artýk geri dönüyoruz ve listeliyoruz.
            }).ToList();
        }

    }
}


// Sayfalama, haber içeriklerini ve modellerini listeleme, arama çubuðu nest ile elastic search
#region
//https://www.elastic.co/docs/reference/elasticsearch/clients/dotnet/examples

// https://stackoverflow.com/questions/33834141/elasticsearch-and-nest-why-am-i-missing-the-id-field-on-a-query?utm_source=chatgpt.com Hits ile Id yakalama
#endregion