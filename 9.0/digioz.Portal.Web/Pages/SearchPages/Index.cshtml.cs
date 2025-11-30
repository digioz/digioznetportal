using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.Bo.ViewModels;
using digioz.Portal.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Pages.SearchPages
{
    public class IndexModel : PageModel
    {
        private readonly IPageService _pageService;
        private readonly IAnnouncementService _announcementService;
        private readonly ICommentService _commentService;
        private readonly IPictureService _pictureService;
        private readonly IVideoService _videoService;
        private readonly ILinkService _linkService;

        public IndexModel(
            IPageService pageService, 
            IAnnouncementService announcementService,
            ICommentService commentService,
            IPictureService pictureService,
            IVideoService videoService,
            ILinkService linkService)
        {
            _pageService = pageService;
            _announcementService = announcementService;
            _commentService = commentService;
            _pictureService = pictureService;
            _videoService = videoService;
            _linkService = linkService;
        }

        [BindProperty(SupportsGet = true)] public string? searchString { get; set; }
        [BindProperty(SupportsGet = true)] public string? searchScope { get; set; } = "All";
        [BindProperty(SupportsGet = true)] public int pageNumber { get; set; } = 1;
        [BindProperty(SupportsGet = true)] public int pageSize { get; set; } = 10;

        public int TotalCount { get; private set; }
        public IReadOnlyList<SearchResultViewModel> Items { get; private set; } = Array.Empty<SearchResultViewModel>();

        public void OnGet()
        {
            var term = (searchString ?? string.Empty).Trim();
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            var skip = (pageNumber - 1) * pageSize;

            if (string.IsNullOrWhiteSpace(searchScope))
                searchScope = "All";

            var results = new List<SearchResultViewModel>();
            var totalCount = 0;

            switch (searchScope)
            {
                case "Page":
                    results = SearchPages(term, skip, pageSize, out totalCount);
                    break;
                case "Announcement":
                    results = SearchAnnouncements(term, skip, pageSize, out totalCount);
                    break;
                case "Comment":
                    results = SearchComments(term, skip, pageSize, out totalCount);
                    break;
                case "Picture":
                    results = SearchPictures(term, skip, pageSize, out totalCount);
                    break;
                case "Video":
                    results = SearchVideos(term, skip, pageSize, out totalCount);
                    break;
                case "Link":
                    results = SearchLinks(term, skip, pageSize, out totalCount);
                    break;
                case "All":
                default:
                    results = SearchAll(term, skip, pageSize, out totalCount);
                    break;
            }

            TotalCount = totalCount;
            Items = results;
        }

        private List<SearchResultViewModel> SearchPages(string term, int skip, int take, out int total)
        {
            var pages = _pageService.Search(term, skip, take, out total);
            return pages.Select(p => MapPageToResult(p, term)).ToList();
        }

        private List<SearchResultViewModel> SearchAnnouncements(string term, int skip, int take, out int total)
        {
            var announcements = _announcementService.Search(term, skip, take, out total);
            return announcements.Select(a => MapAnnouncementToResult(a, term)).ToList();
        }

        private List<SearchResultViewModel> SearchComments(string term, int skip, int take, out int total)
        {
            var comments = _commentService.Search(term, skip, take, out total);
            return comments.Select(c => MapCommentToResult(c, term)).ToList();
        }

        private List<SearchResultViewModel> SearchPictures(string term, int skip, int take, out int total)
        {
            var pictures = _pictureService.Search(term, skip, take, out total);
            return pictures.Select(p => MapPictureToResult(p, term)).ToList();
        }

        private List<SearchResultViewModel> SearchVideos(string term, int skip, int take, out int total)
        {
            var videos = _videoService.Search(term, skip, take, out total);
            return videos.Select(v => MapVideoToResult(v, term)).ToList();
        }

        private List<SearchResultViewModel> SearchLinks(string term, int skip, int take, out int total)
        {
            var links = _linkService.Search(term, skip, take, out total);
            return links.Select(l => MapLinkToResult(l, term)).ToList();
        }

        private List<SearchResultViewModel> SearchAll(string term, int skip, int take, out int total)
        {
            var allResults = new List<SearchResultViewModel>();

            // Search all content types and combine results
            var pages = _pageService.Search(term, 0, int.MaxValue, out _);
            allResults.AddRange(pages.Select(p => MapPageToResult(p, term)));

            var announcements = _announcementService.Search(term, 0, int.MaxValue, out _);
            allResults.AddRange(announcements.Select(a => MapAnnouncementToResult(a, term)));

            var comments = _commentService.Search(term, 0, int.MaxValue, out _);
            allResults.AddRange(comments.Select(c => MapCommentToResult(c, term)));

            var pictures = _pictureService.Search(term, 0, int.MaxValue, out _);
            allResults.AddRange(pictures.Select(p => MapPictureToResult(p, term)));

            var videos = _videoService.Search(term, 0, int.MaxValue, out _);
            allResults.AddRange(videos.Select(v => MapVideoToResult(v, term)));

            var links = _linkService.Search(term, 0, int.MaxValue, out _);
            allResults.AddRange(links.Select(l => MapLinkToResult(l, term)));

            // Sort by timestamp descending
            allResults = allResults.OrderByDescending(r => r.Timestamp).ToList();

            total = allResults.Count;
            return allResults.Skip(skip).Take(take).ToList();
        }

        private SearchResultViewModel MapPageToResult(Bo.Page page, string term)
        {
            var title = page.Title ?? string.Empty;
            var bodyPlain = StringUtils.StripHtmlFromString(page.Body ?? string.Empty);
            var snippet = TruncateDotDotDot(bodyPlain, 200);

            if (!string.IsNullOrWhiteSpace(term))
            {
                title = Highlight(title, term);
                snippet = Highlight(snippet, term);
            }

            var url = page.Url ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(url) && !url.StartsWith("/"))
            {
                url = "/Page/" + url;
            }

            return new SearchResultViewModel
            {
                Id = page.Id,
                TitleHtml = title,
                SnippetHtml = snippet,
                Url = url,
                Timestamp = page.Timestamp
            };
        }

        private SearchResultViewModel MapAnnouncementToResult(Bo.Announcement announcement, string term)
        {
            var title = announcement.Title ?? "Announcement";
            var bodyPlain = StringUtils.StripHtmlFromString(announcement.Body ?? string.Empty);
            var snippet = TruncateDotDotDot(bodyPlain, 200);

            if (!string.IsNullOrWhiteSpace(term))
            {
                title = Highlight(title, term);
                snippet = Highlight(snippet, term);
            }

            return new SearchResultViewModel
            {
                Id = announcement.Id,
                TitleHtml = title,
                SnippetHtml = snippet,
                Url = $"/Announcements/Details?id={announcement.Id}",
                Timestamp = announcement.Timestamp
            };
        }

        private SearchResultViewModel MapCommentToResult(Bo.Comment comment, string term)
        {
            var title = $"Comment by {comment.Username ?? "Anonymous"}";
            var bodyPlain = StringUtils.StripHtmlFromString(comment.Body ?? string.Empty);
            var snippet = TruncateDotDotDot(bodyPlain, 200);

            if (!string.IsNullOrWhiteSpace(term))
            {
                snippet = Highlight(snippet, term);
            }

            return new SearchResultViewModel
            {
                Id = comment.Id,
                TitleHtml = title,
                SnippetHtml = snippet,
                Url = comment.ReferenceType ?? "/",
                Timestamp = comment.ModifiedDate ?? comment.CreatedDate
            };
        }

        private SearchResultViewModel MapPictureToResult(Bo.Picture picture, string term)
        {
            var title = picture.Filename ?? "Picture";
            var snippet = picture.Description ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(term))
            {
                title = Highlight(title, term);
                snippet = Highlight(snippet, term);
            }

            return new SearchResultViewModel
            {
                Id = picture.Id,
                TitleHtml = title,
                SnippetHtml = snippet,
                Url = $"/Pictures/Details?id={picture.Id}",
                Timestamp = picture.Timestamp
            };
        }

        private SearchResultViewModel MapVideoToResult(Bo.Video video, string term)
        {
            var title = video.Filename ?? "Video";
            var snippet = video.Description ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(term))
            {
                title = Highlight(title, term);
                snippet = Highlight(snippet, term);
            }

            return new SearchResultViewModel
            {
                Id = video.Id,
                TitleHtml = title,
                SnippetHtml = snippet,
                Url = $"/Videos/Details?id={video.Id}",
                Timestamp = video.Timestamp
            };
        }

        private SearchResultViewModel MapLinkToResult(Bo.Link link, string term)
        {
            var title = link.Name ?? "Link";
            var snippet = link.Description ?? link.Url ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(term))
            {
                title = Highlight(title, term);
                snippet = Highlight(snippet, term);
            }

            return new SearchResultViewModel
            {
                Id = link.Id,
                TitleHtml = title,
                SnippetHtml = snippet,
                Url = $"/Links?id={link.Id}",
                Timestamp = link.Timestamp
            };
        }

        private static string Highlight(string input, string term)
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(term)) return input;
            try
            {
                var pattern = Regex.Escape(term);
                return Regex.Replace(input, pattern, m => $"<mark>{m.Value}</mark>", RegexOptions.IgnoreCase);
            }
            catch
            {
                return input;
            }
        }

        private static string TruncateDotDotDot(string value, int maxLength)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > maxLength)
            {
                return value.Substring(0, maxLength) + "...";
            }
            return value;
        }
    }
}