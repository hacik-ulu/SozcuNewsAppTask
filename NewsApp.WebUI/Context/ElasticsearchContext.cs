using Elasticsearch.Net;
using Nest;
using NewsApp.WebUI.Dto;

namespace NewsApp.WebUI.Context
{
    public class ElasticsearchContext
    {
        // Elasticsearch sorguları için kullandığımız client
        public ElasticClient Client { get; }

        public ElasticsearchContext(IConfiguration config)
        {
            // elasticsearch cloud bilgilerini çektim.
            var cloudId = config["Elasticsearch:cloudId"];
            var user = config["Elasticsearch:user"];
            var password = config["Elasticsearch:password"];

            // Elasticsearch yapılandırma ayarları cloud ve Index veritabanına bağlanma adına işlemler gerçekleştirdim.
            var configSettings = new ConnectionSettings(cloudId, new BasicAuthenticationCredentials(user, password))
                .DefaultIndex("news-app-demo")
                .DefaultMappingFor<NewsAppDto>(x => x
                    .IndexName("news-app-demo"));

            // Elasticclient örneği oluşturuyoruz.
            Client = new ElasticClient(configSettings);
        }
    }
}



