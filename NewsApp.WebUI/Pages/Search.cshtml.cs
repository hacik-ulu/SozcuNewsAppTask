using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nest;
using NewsApp.WebUI.Dto;

namespace NewsApp.WebUI.Pages
{
    public class SearchModel : PageModel
    {
        private readonly IElasticClient _elasticClient;
        public SearchModel(IElasticClient elasticClient)
        {
            _elasticClient = elasticClient;
        }

        public string Search { get; set; } // aranacak terim.
        public List<NewsAppDto> NewsList { get; set; } = new();

        public async Task OnGetAsync()
        {
            Search = Request.Query["search"]; // tarayıcıdan gelen URL'deki ?search="" parametresini alıp Searche atıyoruz.

            if (string.IsNullOrEmpty(Search))
            {
                return; //metod sonlanır, sayfa tekrar yuklenir;
            }

            // Elasticsearche arama isteği gönderiyoruz SearchAsync() ile, arama kritlerlerini geçiyoruz, birden fazla arama kriteri olduğundan multimatch diyoruz.
            var searchQuery = await _elasticClient.SearchAsync<NewsAppDto>(x => x
                .Query(y => y
                    .MultiMatch(z => z
                        .Fields(t => t
                            .Field(k => k.Title)
                            .Field(w => w.Summary)
                            .Field(p => p.Content)
                            .Field(a => a.Author)
                            .Field(c => c.Category)
                        )
                        .Query(Search) // Kullanıcının girmis oldugu Search değerini bu alanlarda arıyoruz.
                    )
                )
            );

            if (searchQuery.IsValid)
            {
                NewsList = searchQuery.Documents.ToList(); 
            }
        }
    }
}


#region CodeSrc
//https://www.elastic.co/docs/reference/query-languages/query-dsl/query-dsl-multi-match-query --> önemli

//https://www.elastic.co/docs/reference/elasticsearch/clients/dotnet/query

// https://stackoverflow.com/questions/31086987/searching-for-an-input-keyword-in-all-fields-of-an-elasticsearch-document-using
#endregion