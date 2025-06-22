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
            // elasticsearch cloud bilgileri
            var cloudId = config["Elasticsearch:cloudId"];
            var user = config["Elasticsearch:user"];
            var password = config["Elasticsearch:password"];

            // Elasticsearch yapılandırma ayarları cloud ve Index veritabanına bağlanma adına.
            var configSettings = new ConnectionSettings(cloudId, new BasicAuthenticationCredentials(user, password))
                .DefaultIndex("news-app-demo")
                .DefaultMappingFor<NewsAppDto>(x => x
                    .IndexName("news-app-demo"));

            // Elasticclient örneği oluşturuyoruz.
            Client = new ElasticClient(configSettings);
        }
    }
}



#region Code Src
// https://www.elastic.co/docs/reference/elasticsearch/clients/dotnet/_options_on_elasticsearchclientsettings  -->onemli

//https://blexin.com/en/blog-en/how-to-integrate-elasticsearch-in-asp-net-core/

//https://www.elastic.co/docs/reference/elasticsearch/clients/dotnet/connecting

// https://github.com/elastic/elasticsearch-net/issues/8184

#endregion