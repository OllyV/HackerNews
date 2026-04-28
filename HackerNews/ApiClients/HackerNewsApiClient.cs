using Hacker_News.Contracts;
using Hacker_News.Helpers;
using Hacker_News.Interfaces;
using System.Text.Json;

namespace Hacker_News.ApiClients
{
    public sealed class HackerNewsApiClient : IHackerNewsApiClient
    {
        private readonly HttpClient _http;
        private readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            RespectNullableAnnotations = true,
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonDateTimeConverter() }
        };

        public HackerNewsApiClient(HttpClient http) {
            _http = http;
        }

        public async Task<StoryApiClientResponse> GetStoryAsync(int story, CancellationToken ct = default)
        {
            var url = $"v0/item/{story.ToString()}.json";
            using var response = await _http.GetAsync(url, ct);
            response.EnsureSuccessStatusCode();

            var text = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(text))
                throw new InvalidOperationException($"API returned empty response for story {story}.");

            var payload = JsonSerializer.Deserialize<StoryApiClientResponse>(text, _options)
                ?? throw new InvalidOperationException($"Deserialization failed for story {story}.");

            return payload;
        }

        public async Task<IEnumerable<int>> GetBestStoriesIdsAsync(CancellationToken ct = default)
        {
            var url = $"v0/beststories.json";
            using var response = await _http.GetAsync(url, ct);
            response.EnsureSuccessStatusCode();

            var text = await response.Content.ReadAsStringAsync();
            var payload = JsonSerializer.Deserialize<IEnumerable<int>>(text, _options);

            if (payload is null)
                throw new InvalidOperationException("API returned empty response.");

            return payload;
        }

    }
}
