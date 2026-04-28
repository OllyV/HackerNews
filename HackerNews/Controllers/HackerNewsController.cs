using Hacker_News.Contracts;
using Hacker_News.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hacker_News.Controllers
{
    
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class HackerNewsController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IHackerNewsService _service;

        public HackerNewsController(ILogger<HackerNewsController> logger,
            IHackerNewsService service)
        {
            _logger = logger;
            _service = service;
        }

        [HttpGet("{storyId:int}")]
        public async Task<IActionResult> Get(int storyId)
        {
            try 
            {
                if (storyId <= 0)
                {
                    return BadRequest("Story ID must be greater than 0.");
                }
    
                var result = await _service.GetStoryAsync(storyId);
                if (result == null)
                {
                    _logger.LogWarning("No content returned.");
                    return NoContent();
                }

                return HandleResult(result);
            
            } catch (Exception ex) {
                return HandleException(ex);
            }
        }


        [HttpGet("best")]
        [ResponseCache(Duration = 300, VaryByHeader = "Accept")]
        public async Task<IActionResult> GetBestStoriesIds(CancellationToken ct = default)
        {
            try
            {
                var result = await _service.GetBestStoriesIdsAsync();
                return HandleResult(result);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("best/{count:int}")]
        public async Task<IActionResult> GetBestStories(int count, CancellationToken ct = default)
        {
            try
            {
                if (count <= 0 || count > 200)
                    return BadRequest("Count must be between 1 and 200.");

                var result = await _service.GetBestStoriesAsync(count, ct);
                return HandleResult(result);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        private IActionResult HandleResult<T>(T? result)
        {
            if (result == null)
            {
                _logger.LogWarning("No content returned.");
                return NoContent();
            }
            return Ok(result);
        }

        private IActionResult HandleException(Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request.");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                $"An error occurred while processing the request. {ex.Message}");
        }
    }
}
