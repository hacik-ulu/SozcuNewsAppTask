using Nest;
using NewsApp.Api.Elasticsearch.Abstract;
using NewsApp.Api.Models;
using System.Runtime.ExceptionServices;
using System.Threading;

namespace NewsApp.Api.Elasticsearch.Concrete
{
    public class ElasticNewsService : IElasticNewsService
    {
        private readonly IElasticClient _elasticClient; //Nest ile elasticsearch ile iletisim kuruyoruz, ayrıca burada program.csdeki deploy bilgileri(secret.jsondan alınan) tutuluyor.
        private readonly ILogger<ElasticNewsService> _logger; // Uygulama kayıtları tutuyoruz.
        private readonly HttpClient _httpClientService; // apiye erisim icin httpClient istemcisi olusturdum.
        private readonly string _newsApiUrl;

        public ElasticNewsService(IElasticClient elasticClient, ILogger<ElasticNewsService> logger, HttpClient httpClientService, IConfiguration configuration)
        {
            _elasticClient = elasticClient;
            _logger = logger;
            _httpClientService = httpClientService;
            _newsApiUrl = configuration["NewsApi:Url"]; // appsettings.jsondan verileri alıyorum

        }

        public async Task BulkNewsFromApiAsync()
        {
            try
            {
                // apiden haber verilerini NewsAppDto ile eşleyip json olarak çekiyoruz.
                var news = await _httpClientService.GetFromJsonAsync<List<NewsAppDto>>(_newsApiUrl);

                if (news == null)
                {
                    _logger.LogWarning("Haber Listesi boş !");
                }

                if (news.Count == 0)
                {
                    _logger.LogWarning("Herhangi haber içeriği bulunamadı !");
                }

                // İptal islemi
                var cancellationToken = CancellationToken.None;

                // Elasticsearche bulk etme toplu, verilerin elasticsearche belirtilen indexe(tabloya gibi) yüklenmesini sağladım.
                var bulkNews = _elasticClient.BulkAll(news,
                      x => x
                          .Index("news-app-demo") // Program.cs deki Index konumu
                          .BackOffRetries(2) // Hata durumunda kaç defa bulk deneneceği
                          .BackOffTime("3s") // Kaç saniye arayla tekrar deneneceği
                          .ContinueAfterDroppedDocuments() // geçersiz ya da hatalı veri varsa tüm islemleri durdurmamak devam etmek adına ekledim.
                          .MaxDegreeOfParallelism(2) // Aynı anda kaç bulk isteği gönderebileceğimiz
                          .Size(50), // İşlem/Haber sayısı
                      cancellationToken);

                // Bulk(1) işleminin tamamlanmasını bekliyoruz.
                var waitHandleProcess = new CountdownEvent(1);
                ExceptionDispatchInfo? exceptionDispatchInfo = null; //try-catche girmeden orjinal haliyle hata varsa fırlatılır.

                // Elasticsearche veri kaydetme/gönderme (bulk) işlemini izliyoruz, işlemin başarılı ya da başarısız olma durumunu gözlemliyorum.
                bulkNews.Subscribe(new BulkAllObserver(
                       // Her parça veri başarılı gonderilirse logluyoruz
                       // Batch işlendi --> Veri elasticsearch veritabanına kaydedildi
                       onNext: y => _logger.LogInformation("Batch işlendi."),
                       onError: error =>
                       {
                           // Burada bulk işlemi devam ederken mevcutta bir hata olursa direkt hatayı saklayıp fırlatıyor.

                           _logger.LogError(error, "Elasticsearch Bulk hatası.");
                           exceptionDispatchInfo = ExceptionDispatchInfo.Capture(error); // Hatanın detaylarını saklıyoruz
                           waitHandleProcess.Signal(); // İşi durduruyoruz. İşi tamamladığına dair sinyali veriyoruz.
                           // Her sinyal çağrısında CountdownEvent(yapılacak iş sayacı) 1 azaltılır.
                       },
                       onCompleted: () => waitHandleProcess.Signal() // Tüm veriler başarıyla gönderilirse, işlem tamamlanmış olur ve sistem çalışmaya devam edebilir.İşi tamamladığına dair sinyali veriyoruz.
                   ));

                // Verilerin bulk işlemi için başarılı/başarısız durumunu 10 dakika kadar bekler, sonrasında zaman aşımına uğraycak.
                waitHandleProcess.Wait(TimeSpan.FromMinutes(10), cancellationToken);

                if (exceptionDispatchInfo != null && exceptionDispatchInfo.SourceException is not OperationCanceledException)
                {
                    // Hata daha onceden yakalandıysa ve iptal edilmediyse orijinal haliyle hata fırlatılır.
                    exceptionDispatchInfo.Throw();
                }
            }

            // Beklenmeyen herhangi bir hata olursa hata loglanır.
            catch (Exception exception)
            {
                // Bulk işleminin sonuclanmasından sonra aynı zamanda beklenmeyen hataları da dahil edip hatayı/hataları fırlatıyor.
                _logger.LogError(exception, "Çekilen verilerin elasticsearch veritabanına kayıt sürecinde hata oluştu !");
            }



        }





    }

}



