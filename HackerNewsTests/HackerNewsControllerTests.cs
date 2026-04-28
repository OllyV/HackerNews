using Hacker_News.Controllers;
using Hacker_News.Contracts;
using Hacker_News.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace HackerNewsTests
{
    public class HackerNewsControllerTests
    {
        private readonly Mock<ILogger<HackerNewsController>> _mockLogger;
        private readonly Mock<IHackerNewsService> _mockService;
        private readonly HackerNewsController _controller;

        public HackerNewsControllerTests()
        {
            _mockLogger = new Mock<ILogger<HackerNewsController>>();
            _mockService = new Mock<IHackerNewsService>();
            _controller = new HackerNewsController(_mockLogger.Object, _mockService.Object);
        }

        #region Get (Single Story) Tests

        [Fact]
        public async Task Get_WithValidStoryId_ReturnsOkResultWithStory()
        {
            // Arrange
            var storyId = 12345;
            var expectedStory = new Story
            {
                Title = "Test Story",
                PostedBy = "testuser",
                Score = 100,
                Time = new DateTime(2024, 1, 1),
                Type = "story",
                Uri = "https://example.com",
                CommentsCount = "5"
            };

            _mockService
                .Setup(s => s.GetStoryAsync(storyId))
                .ReturnsAsync(expectedStory);

            // Act
            var result = await _controller.Get(storyId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<Story>(okResult.Value);
            Assert.Equal(expectedStory.Title, returnValue.Title);
            Assert.Equal(expectedStory.PostedBy, returnValue.PostedBy);
            Assert.Equal(expectedStory.Score, returnValue.Score);
            Assert.Equal(expectedStory.Uri, returnValue.Uri);
        }

        [Fact]
        public async Task Get_WithValidStoryId_CallsServiceWithCorrectId()
        {
            // Arrange
            var storyId = 54321;
            var story = new Story { Title = "Test" };

            _mockService
                .Setup(s => s.GetStoryAsync(storyId))
                .ReturnsAsync(story);

            // Act
            await _controller.Get(storyId);

            // Assert
            _mockService.Verify(s => s.GetStoryAsync(storyId), Times.Once());
        }

        [Fact]
        public async Task Get_WithNegativeStoryId_ReturnsBadRequest()
        {
            // Arrange
            var storyId = -1;

            // Act
            var result = await _controller.Get(storyId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Story ID must be greater than 0.", badRequestResult.Value);
            _mockService.Verify(s => s.GetStoryAsync(It.IsAny<int>()), Times.Never());
        }

        #endregion

        #region GetBestStoriesIds Tests

        [Fact]
        public async Task GetBestStoriesIds_WithValidData_ReturnsOkResultWithIds()
        {
            // Arrange
            var expectedIds = new List<int> { 12345, 54321, 11111, 22222, 33333 };

            _mockService
                .Setup(s => s.GetBestStoriesIdsAsync())
                .ReturnsAsync(expectedIds);

            // Act
            var result = await _controller.GetBestStoriesIds();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<List<int>>(okResult.Value);
            Assert.Equal(expectedIds, returnValue);
        }

        [Fact]
        public async Task GetBestStoriesIds_WithServiceException_LogsError()
        {
            // Arrange
            var exception = new InvalidOperationException("Test error");

            _mockService
                .Setup(s => s.GetBestStoriesIdsAsync())
                .ThrowsAsync(exception);

            // Act
            await _controller.GetBestStoriesIds();

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once());
        }

        #endregion

        #region GetBestStories Tests

        [Fact]
        public async Task GetBestStories_WithValidCount_ReturnsOkResultWithStories()
        {
            // Arrange
            var count = 10;
            var expectedStories = new List<Story>
            {
                new Story { Title = "Story 1", Score = 100 },
                new Story { Title = "Story 2", Score = 90 }
            };

            _mockService
                .Setup(s => s.GetBestStoriesAsync(count))
                .ReturnsAsync(expectedStories);

            // Act
            var result = await _controller.GetBestStories(count);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<List<Story>>(okResult.Value);
            Assert.Equal(expectedStories.Count, returnValue.Count);
            Assert.Equal(expectedStories[0].Title, returnValue[0].Title);
        }

        [Fact]
        public async Task GetBestStories_WithNegativeCount_ReturnsBadRequest()
        {
            // Arrange
            var count = -5;

            // Act
            var result = await _controller.GetBestStories(count);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Count must be between 1 and 200.", badRequestResult.Value);
            _mockService.Verify(s => s.GetBestStoriesAsync(It.IsAny<int>()), Times.Never());
        }

        [Fact]
        public async Task GetBestStories_WithServiceException_LogsError()
        {
            // Arrange
            var count = 10;
            var exception = new InvalidOperationException("Test error");

            _mockService
                .Setup(s => s.GetBestStoriesAsync(count))
                .ThrowsAsync(exception);

            // Act
            await _controller.GetBestStories(count);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once());
        }
        #endregion
    }
}