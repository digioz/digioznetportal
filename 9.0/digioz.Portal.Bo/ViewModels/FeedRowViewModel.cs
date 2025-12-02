namespace digioz.Portal.Bo.ViewModels
{
    public class FeedRowViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public int MaxCount { get; set; }
    }
}
