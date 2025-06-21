//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
//using Nest;
//using NewsApp.Api.Elasticsearch.Abstract;
//using System;
//using System.Runtime.ExceptionServices;
//using System.Threading;
//using System.Threading.Tasks;

//namespace NewsApp.Api.Workers
//{
//    public class TestNews : BackgroundService
//    { 



// VideoLink -->https://www.youtube.com/watch?v=tw9svKWq6tg&t=634s&ab_channel=dotnetFlix


//        private readonly ILogger<TestNews> _logger;
//        private readonly IElasticClient _client;
//        private readonly INewsDataReader _newsDataReader; // Haber verilerini okuyan bir sınıf olduğunu varsayıyoruz

//        public TestNews(ILogger<TestNews> logger, IElasticClient client, INewsDataReader newsDataReader)
//        {
//            _logger = logger;
//            _client = client;
//            _newsDataReader = newsDataReader;
//        }

//        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//        {
//            // Elasticsearche verileri topluca göndermek için bulk işlemi yapıyoruz.
//            // Verileri CSV dosyasından okuyup IEnumerable<NewsAppDto> şeklinde alıyoruz.
//            var bulkAll = _client.BulkAll(
//              _newsDataReader.GetNews(stoppingToken), // IEnumerable<T> dönen haber verisi kaynağın
//              b => b
//                  .Index("news-index-v1") // verileri yükleyeceğimiz indexin adı
//                  .BackOffRetries(2) // Yükleme durumunda hata olursa kaç defa yeniden denenecek
//                  .BackOffTime("3s") // Yeniden denemelerde bekleme aralığı süresi
//                  .MaxDegreeOfParallelism(4) // Aynı anda kaç parça çalışacak
//                  .Size(1000), // Her işlemde kaç belge gönderilecek
//              stoppingToken
//          );

//            var waitHandle = new CountdownEvent(1); // Bulk işleminin tamamlanmasını beklemek adına CountdownEvent kullanıyoruz.

//            // Hata durumu
//            ExceptionDispatchInfo captureInfo = null;

//            // BulkAll işlemini subscribe edip (başlatıp) dinlendirmeye alıyoruz
//            var subscription = bulkAll.Subscribe(new BulkAllObserver(
//                onNext: b => _logger.LogInformation("Data indexed"), // Datayı logluyoruz
//                onError: e =>
//                {
//                    // Hata durumunda yakalayp bekleme sinyali veriyoruz.
//                    captureInfo = ExceptionDispatchInfo.Capture(e);
//                    waitHandle.Signal(); // Tüm işlem başarıyla biterse sinyal veriyoruz.
//                },
//                onCompleted: () => waitHandle.Signal()
//            ));

//            waitHandle.Wait(TimeSpan.FromMinutes(30), stoppingToken); // Tüm işlemin bitmesibni 30 dk bekilyoruz.

//            // Eğer işlem sırasında bir hata olduysa ve bu iptal değilse, hatayı yeniden fırlatıyoruz
//            if (captureInfo != null && captureInfo.SourceException is not OperationCanceledException)
//                captureInfo?.Throw();

//            // news-index ile sorgu yapıldığında news-index-v1'e içeriğine geçiş yapmış oluruz.
//            await _client.Indices.PutAliasAsync("news-index-v1", "news-index", ct: stoppingToken);
//        }

//    }
//}




//#region Test Data Insert 
////var exampleNewsData = new
////{
////    Title = "Test Title",
////    Content = "This is a sample news content for elasticsearch"
////};

////var response = await _client.IndexDocumentAsync(exampleNewsData, stoppingToken);

////if (response.IsValid)
////{
////    _logger.LogInformation(response.Id);
////}

//#endregion