using NewsApp.Api.Models;

namespace NewsApp.Api.Services.Abstract
{
    public interface INewsAppService
    {
        Task<List<NewsAppDto>> GetNewsListAsync()
    }
}
