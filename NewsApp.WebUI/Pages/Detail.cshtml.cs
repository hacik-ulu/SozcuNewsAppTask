using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsApp.WebUI.Context;
using NewsApp.WebUI.Dto;

namespace NewsApp.WebUI.Pages
{
    public class DetailModel : PageModel
    {
        private readonly ElasticsearchContext _context;
        public NewsAppDto NewsDetail { get; set; }
        
        public DetailModel(ElasticsearchContext context)
        {
            _context = context;
        }

        public async Task OnGetAsync ()
        {
        }
    }
}
