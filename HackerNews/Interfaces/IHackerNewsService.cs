using Hacker_News.Contracts;

namespace Hacker_News.Interfaces
{
    public interface IHackerNewsService
    {
        Task<Story?> GetStoryAsync(int story, CancellationToken ct = default);
        Task<IEnumerable<int>?> GetBestStoriesIdsAsync(CancellationToken ct = default);
        Task<IEnumerable<Story>> GetBestStoriesAsync(int count, CancellationToken ct = default);
    }
}
