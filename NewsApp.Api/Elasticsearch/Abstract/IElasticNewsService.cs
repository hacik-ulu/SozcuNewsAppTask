namespace NewsApp.Api.Elasticsearch.Abstract
{
    public interface IElasticNewsService
    {
        Task BulkNewsFromApiAsync();
    }
}
