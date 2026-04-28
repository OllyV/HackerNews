using Hacker_News.ApiClients;
using Hacker_News.Contracts;
using Hacker_News.Helpers;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;

namespace HackerNewsTests
{
    public class HackerNewsApiClientTests
    {
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly HttpClient _httpClient;
        private readonly HackerNewsApiClient _apiClient;

        public HackerNewsApiClientTests()
        {
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://hacker-news.firebaseio.com/"),
                Timeout = System.Threading.Timeout.InfiniteTimeSpan
            };
            _apiClient = new HackerNewsApiClient(_httpClient);
        }

        #region GetStoryAsync Tests

        [Fact]
        public async Task GetStoryAsync_WithValidStoryId_ReturnsStoryApiClientResponse()
        {
            // Arrange
            var storyId = 12345;
            var storyResponse = new StoryApiClientResponse
            {
                Id = storyId,
                By = "testuser",
                Score = 100,
                Title = "Test Story",
                Type = "story",
                Url = "https://example.com",
                Time = new DateTime(2024, 1, 1),
                Descendants = 5,
                Kids = new List<int> { 1, 2, 3 }
            };

            var options = new JsonSerializerOptions
            {
                RespectNullableAnnotations = true,
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonDateTimeConverter() }
            };

            var jsonContent = JsonSerializer.Serialize(storyResponse, options);
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonContent)
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri.ToString().Contains($"v0/item/{storyId}.json")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            // Act
            var result = await _apiClient.GetStoryAsync(storyId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(storyId, result.Id);
            Assert.Equal("testuser", result.By);
            Assert.Equal(100, result.Score);
            Assert.Equal("Test Story", result.Title);
        }

        [Fact]
        public async Task GetStoryAsync_WithHttpError_ThrowsHttpRequestException()
        {
            // Arrange
            var storyId = 12345;
            var responseMessage = new HttpResponseMessage(HttpStatusCode.NotFound);

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => _apiClient.GetStoryAsync(storyId));
        }

        [Fact]
        public async Task GetStoryAsync_WithNullableProperties_DeserializesCorrectly()
        {
            // Arrange
            var storyId = 12345;
            var jsonContent = "{\"id\":12345,\"by\":null,\"score\":50,\"title\":\"Test\",\"type\":\"story\"}";
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonContent)
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            // Act
            var result = await _apiClient.GetStoryAsync(storyId);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.By);
            Assert.Equal(50, result.Score);
        }

        #endregion

        #region GetBestStoriesIdsAsync Tests

        [Fact]
        public async Task GetBestStoriesIdsAsync_WithValidResponse_ReturnsListOfIds()
        {
            // Arrange
            var expectedIds = new[] { 12345, 54321, 11111, 22222, 33333 };
            var jsonContent = JsonSerializer.Serialize(expectedIds);
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonContent)
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri.ToString().Contains("v0/beststories.json")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            // Act
            var result = await _apiClient.GetBestStoriesIdsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedIds, result);
        }

        #endregion
    }
}