namespace Hacker_News.Contracts
{
    public sealed class StoryApiClientResponse
    {
        public string? By { get;  set; }
        public int? Descendants { get; set; }
        public int? Id { get; set; }
        public IEnumerable<int>? Kids { get; set; }
        public int? Score { get; set; }
        public DateTime? Time { get; set; }
        public string? Title { get; set; }
        public string? Type { get; set; }
        public string? Url { get; set; }
        public string? Text { get; set; }

    }
}
