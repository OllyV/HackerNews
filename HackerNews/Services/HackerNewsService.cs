using System.Collections.Concurrent;
using AutoMapper;
using Hacker_News.ApiClients;
using Hacker_News.Contracts;
using Hacker_News.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Hacker_News.Services
{
    public sealed class HackerNewsService: IHackerNewsService
    {
        private readonly IHackerNewsApiClient _hackerNewsApiClient;
        private readonly IMemoryCache _memoryCache;
        private readonly IMapper _mapper;

        public HackerNewsService(IHackerNewsApiClient hackerNewsApiClient, IMemoryCache memoryCache, IMapper mapper) {
            _hackerNewsApiClient = hackerNewsApiClient;
            _memoryCache = memoryCache;
            _mapper = mapper;
        }

        public async Task<Story?> GetStoryAsync(int story, CancellationToken ct = default)
        {
            return await _memoryCache.GetOrCreateAsync(story, async entry =>
            {
                entry.SetSlidingExpiration(TimeSpan.FromMinutes(5))
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(10))
                    .SetPriority(CacheItemPriority.High)
                    .SetSize(1);
                return _mapper.Map<Story>(await _hackerNewsApiClient.GetStoryAsync(story, ct));
            });
        }

        public async Task<IEnumerable<int>?> GetBestStoriesIdsAsync(CancellationToken ct = default)
        {
            return await _memoryCache.GetOrCreateAsync("best", async entry =>
            {
                entry.SetSlidingExpiration(TimeSpan.FromMinutes(5))
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(15))
                    .SetPriority(CacheItemPriority.High)
                    .SetSize(1);
                return (await _hackerNewsApiClient.GetBestStoriesIdsAsync(ct)).OrderByDescending(x=>x);
            });
        }

        public async Task<IEnumerable<Story>> GetBestStoriesAsync(int count, CancellationToken ct = default)
        {
            IEnumerable<int> numbers = await GetBestStoriesIdsAsync(ct) ?? Array.Empty<int>();
            numbers = numbers.Take(count);

            var stories = new ConcurrentBag<Story>();
            using var semaphore = new SemaphoreSlim(10);
            var tasks = numbers.Select(async storyId =>
            {
                await semaphore.WaitAsync(ct);
                try
                {
                    var story = await GetStoryAsync(storyId, ct);
                    if (story != null)
                        stories.Add(story);
                }
                finally
                {
                    semaphore.Release();
                }
            });
            await Task.WhenAll(tasks);
            return stories;
        }
    }
}
