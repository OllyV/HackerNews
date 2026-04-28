using Hacker_News.Contracts;

namespace Hacker_News.Interfaces
{
    public interface IHackerNewsApiClient
    {
        Task<StoryApiClientResponse> GetStoryAsync(int story, CancellationToken ct = default);
        Task<IEnumerable<int>> GetBestStoriesIdsAsync(CancellationToken ct = default);
    }
}
