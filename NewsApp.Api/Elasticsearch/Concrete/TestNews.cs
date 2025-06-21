using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nest;
using System.Threading;
using System.Threading.Tasks;

namespace NewsApp.Api.Workers
{
    public class TestNews : BackgroundService
    {
        private readonly ILogger<TestNews> _logger;
        private readonly IElasticClient _client;

        public TestNews(ILogger<TestNews> logger, IElasticClient client)
        {
            _logger = logger;
            _client = client;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var exampleNewsData = new
            {
                Title = "Test Title",
                Content = "This is a sample news content for elasticsearch"
            };

            var response = await _client.IndexDocumentAsync(exampleNewsData, stoppingToken);

            if (response.IsValid)
            {
                _logger.LogInformation(response.Id);
            }
        }
    }
}
