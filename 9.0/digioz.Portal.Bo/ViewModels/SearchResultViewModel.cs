using System;

namespace digioz.Portal.Bo.ViewModels
{
    public class SearchResultViewModel
    {
        public object Id { get; set; }
        public string TitleHtml { get; set; } = string.Empty;
        public string SnippetHtml { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public DateTime? Timestamp { get; set; }
    }
}
