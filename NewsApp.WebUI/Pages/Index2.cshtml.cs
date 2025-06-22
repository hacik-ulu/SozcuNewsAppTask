using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsApp.WebUI.Context;

namespace NewsApp.WebUI.Pages
{
    public class Index2Model : PageModel
    {
        private readonly ElasticsearchContext _elastic;
        public string Message { get; set; } = string.Empty;

        public Index2Model(ElasticsearchContext elastic)
        {
            _elastic = elastic;
        }

        public void OnGet()
        {
            var pingResponse = _elastic.Client.Ping(); // Elasticsearch'e basit baðlantý testi

            Message = pingResponse.IsValid
                ? " ElasticSearch baðlantýsý baþarýlý."
                : $" Baðlantý baþarýsýz: {pingResponse.OriginalException.Message}";
        }
    }
}
