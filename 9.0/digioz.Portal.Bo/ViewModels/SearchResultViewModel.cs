namespace digioz.Portal.Bo.ViewModels
{
    public class SearchResultViewModel
    {
        public int Id { get; set; }
        public string TitleHtml { get; set; } = string.Empty;
        public string SnippetHtml { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }
}
