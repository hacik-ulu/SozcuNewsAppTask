using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsApp.WebUI.Context;
using NewsApp.WebUI.Dto;

namespace NewsApp.WebUI.Pages
{
    public class DetailModel : PageModel
    {
        private readonly ElasticsearchContext _elasticsearchContext;
        public NewsAppDto NewsDetail { get; set; }
        public NewsAppDto News { get; set; }


        public DetailModel(ElasticsearchContext context)
        {
            _elasticsearchContext = context;
        }

        public async Task OnGetAsync(string id)
        {
            var news = await _elasticsearchContext.Client.GetAsync<NewsAppDto>(id, x => x.Index("news-app-demo"));

            News = news.Source;
            News.Id = news.Id;
        }
    }
}
