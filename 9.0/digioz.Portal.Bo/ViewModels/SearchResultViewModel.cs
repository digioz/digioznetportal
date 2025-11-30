using System;

namespace digioz.Portal.Bo.ViewModels
{
    public class SearchResultViewModel
    {
        /// <summary>
        /// String representation of the content ID for routing/display purposes.
        /// This accommodates different ID types (int, string) across content types.
        /// </summary>
        public string Id { get; set; } = string.Empty;
        
        public string TitleHtml { get; set; } = string.Empty;
        public string SnippetHtml { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public DateTime? Timestamp { get; set; }
        
        /// <summary>
        /// Content type identifier (Page, Announcement, Comment, Picture, Video, Link)
        /// </summary>
        public string ContentType { get; set; } = string.Empty;
    }
}
