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
            Search = Request.Query["query"]; // URL'den gelen arama parametresini alacak

            if (string.IsNullOrWhiteSpace(Search))
            {
                return; // boşsa işlem yapmicak.
            }

            var normalizedSearch = Normalize(Search); // arama parametresi türkçe ve büyük harf küçük harf uyumuna göre normalize ediyoruz.

            //multimatch --> bir arama sorgusunun birden fazla fieldda kontrol edilmesini saglıyor.
            var searchQuery = await _elasticClient.SearchAsync<NewsAppDto>(s => s
                .Query(q => q
                    .Bool(b => b // birden fazla sorguyu bir araya getirmek adına kullanıyoruz.
                        .Should(
                            // Fuzzy destekli MultiMatch araması islemmini yapiyoruz (orijinal hali)
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
                                .PrefixLength(1) // benzer harf durumu en az bir tane bile eşleşebilir şu anda
                                .MaxExpansions(30) // kaç farklı varyasyon deniyoruz
                            ),
                            // Normalize edilmiş arama (büyük/küçük harf uyumu ve Türkçe karakter dönüşüm islemlerini kontrol editoruz)
                            m => m.MultiMatch(mm => mm
                                .Fields(f => f
                                    .Field(p => p.Title)
                                    .Field(p => p.Summary)
                                    .Field(p => p.Content)
                                    .Field(p => p.Author)
                                    .Field(p => p.Category)
                                )
                                .Query(normalizedSearch)
                                .Fuzziness(Fuzziness.Auto)
                                .PrefixLength(1)
                                .MaxExpansions(30)
                            ),
                            // PhrasePrefix ile eşleşmenin başından itibaren eşleşme kontrolü
                            // Başharfinden itibaren otomatik eslesen var mı diye PhrasePrefix ile kontrol sagliyoruz.
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
                NewsList = searchQuery.Hits.Select(hit =>
                {
                    var documentary = hit.Source;
                    documentary.Id = hit.Id;
                    return documentary;
                }).ToList();
            }
        }


        private string Normalize(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            var normalized = input.ToLower(new System.Globalization.CultureInfo("tr-TR"));

            return normalized
                .Replace("ç", "c")
                .Replace("ğ", "g")
                .Replace("ı", "i")
                .Replace("ö", "o")
                .Replace("ş", "s")
                .Replace("ü", "u");
        }


    }
}




