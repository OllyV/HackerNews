using AutoMapper;
using Hacker_News.Contracts;
using Hacker_News.Helpers;
using Hacker_News.Interfaces;
using Hacker_News.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace HackerNewsTests
{
    public class HackerNewsServiceTests
    {
        private readonly Mock<IHackerNewsApiClient> _mockApiClient;
        private readonly IMemoryCache _cache;
        private readonly IMapper _mapper;
        private readonly HackerNewsService _service;

        public HackerNewsServiceTests()
        {
            _mockApiClient = new Mock<IHackerNewsApiClient>();
            _cache = new MemoryCache(new MemoryCacheOptions());
            _mapper = new MapperConfiguration(cfg => cfg.AddProfile<StoryProfile>(), NullLoggerFactory.Instance).CreateMapper();
            _service = new HackerNewsService(_mockApiClient.Object, _cache, _mapper);
        }

        [Fact]
        public async Task GetStoryAsync_ReturnsCorrectlyMappedStory()
        {
            var apiResponse = new StoryApiClientResponse
            {
                Id = 1,
                By = "alice",
                Score = 42,
                Title = "Test Title",
                Url = "https://example.com",
                Time = new DateTime(2024, 1, 1),
                Type = "story",
                Kids = new List<int> { 10, 20, 30 }
            };
            _mockApiClient
                .Setup(c => c.GetStoryAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(apiResponse);

            var result = await _service.GetStoryAsync(1);

            Assert.NotNull(result);
            Assert.Equal("alice", result.PostedBy);
            Assert.Equal(42, result.Score);
            Assert.Equal("https://example.com", result.Uri);
            Assert.Equal("3", result.CommentsCount);
        }

        [Fact]
        public async Task GetStoryAsync_CachesResult_ApiCalledOnce()
        {
            var apiResponse = new StoryApiClientResponse { Id = 1, Kids = new List<int>() };
            _mockApiClient
                .Setup(c => c.GetStoryAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(apiResponse);

            await _service.GetStoryAsync(1);
            await _service.GetStoryAsync(1);

            _mockApiClient.Verify(c => c.GetStoryAsync(1, It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task GetBestStoriesIdsAsync_ReturnsSortedDescending()
        {
            _mockApiClient
                .Setup(c => c.GetBestStoriesIdsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<int> { 10, 50, 30, 20 });

            var result = await _service.GetBestStoriesIdsAsync();

            Assert.Equal(new[] { 50, 30, 20, 10 }, result);
        }

        [Fact]
        public async Task GetBestStoriesIdsAsync_CachesResult_ApiCalledOnce()
        {
            _mockApiClient
                .Setup(c => c.GetBestStoriesIdsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<int> { 1, 2, 3 });

            await _service.GetBestStoriesIdsAsync();
            await _service.GetBestStoriesIdsAsync();

            _mockApiClient.Verify(c => c.GetBestStoriesIdsAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task GetBestStoriesAsync_ReturnsRequestedCount()
        {
            _mockApiClient
                .Setup(c => c.GetBestStoriesIdsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Enumerable.Range(1, 10).ToList());
            _mockApiClient
                .Setup(c => c.GetStoryAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int id, CancellationToken _) =>
                    new StoryApiClientResponse { Id = id, Kids = new List<int>() });

            var result = await _service.GetBestStoriesAsync(3);

            Assert.Equal(3, result.Count());
        }
    }
}
