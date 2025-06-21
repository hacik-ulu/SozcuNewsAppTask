using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewsApp.Api.Elasticsearch.Abstract;

namespace NewsApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsBulksController : ControllerBase
    {
        private readonly IElasticNewsService _elasticNewsService;

        public NewsBulksController(IElasticNewsService elasticNewsService)
        {
            _elasticNewsService = elasticNewsService;
        }

        [HttpPost("bulk-news")]
        public async Task<IActionResult> BulkNews()
        {
            try
            {
                await _elasticNewsService.BulkNewsFromApiAsync();
                return Ok(" Bulk operation completed successfully.");
            }
            catch (Exception exception)
            {
                return StatusCode(500, $" An error occurred during bulk operation: {exception.Message}");
            }
        }

    }
}
