namespace Hacker_News.Contracts
{
    public sealed class Story
    {
        public string? PostedBy { get; set; }
        public int Score { get; set; }
        public DateTime Time { get; set; }
        public string? Title { get; set; }
        public string? Type { get; set; }
        public string? Uri    { get; set; }
        public string? Text { get; set; }
        public string? CommentsCount { get; set; }

    }
}
