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
            Search = Request.Query["search"]; // URL'den gelen arama parametresini al

            if (string.IsNullOrWhiteSpace(Search))
            {
                return; // boşsa işlem yapma
            }

            //multimatch --> bir arama sorgusunun birden fazla fieldda kontrol edilmesini saglıyor.


            var searchQuery = await _elasticClient.SearchAsync<NewsAppDto>(s => s
                .Query(q => q
                    .Bool(b => b // birden fazla sorguyu bir araya getirmek adına kullanıyoruz.
                        .Should(
                            // Fuzzy destekli MultiMatch araması
                            m => m.MultiMatch(mm => mm  // Örneğin ilkay yazdığımızda tüm fieldlar için (multimatch) kontrol sağlıyoruz.
                                .Fields(f => f
                                    .Field(p => p.Title)
                                    .Field(p => p.Summary)
                                    .Field(p => p.Content)
                                    .Field(p => p.Author)
                                    .Field(p => p.Category)
                                )
                                .Query(Search)
                                .Fuzziness(Fuzziness.Auto) // yazım yanlısları da olsa esneklik sağlıyoruz
                                .PrefixLength(1) // benzer harf durumu
                                .MaxExpansions(30) // kaç farklı varyasyon deniyoruz
                            ),
                            // Kelimenin yanlıs yazımı dısında kelimenin en basından eslestirilmesi islemi adına orneğin ilk yazıp ilkay sonuclar gelmesi adına TextQueryType kullanıyoruz.
                            m => m.MultiMatch(mm => mm
                                .Fields(f => f
                                    .Field(p => p.Title)
                                    .Field(p => p.Summary)
                                    .Field(p => p.Content)
                                    .Field(p => p.Author)
                                    .Field(p => p.Category)
                                )
                                .Query(Search)
                                .Type(TextQueryType.PhrasePrefix)
                            )
                        )
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

//https://www.elastic.co/docs/reference/query-languages/query-dsl/query-dsl-fuzzy-query  --> fuzzy query ve max expansions

//https://www.pipiho.com/es/7.7/en/query-dsl-multi-match-query.html

//https://stackoverflow.com/questions/74867381/elasticsearch-partial-search-with-fuzziness-on-multiple-fields   --> Multimatch ile iki ayrı clause (yazım yanlısı ve kelime eslesmesi durumu)


#endregion