using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewsApp.Api.Services.Abstract;

namespace NewsApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsController : ControllerBase
    {
        private readonly INewsAppService _newsAppService;
        public NewsController(INewsAppService newsAppService)
        {
            _newsAppService = newsAppService;
        }

        [HttpGet("get-news")]
        public async Task<IActionResult> GetNews()
        {
            var news = await _newsAppService.GetNewsListAsync();
            return Ok(news);
        }
    }
}
