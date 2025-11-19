using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.Bo.ViewModels;
using digioz.Portal.Utilities; // Added for extension methods
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Pages.SearchPages
{
    public class IndexModel : PageModel
    {
        private readonly IPageService _pageService;
        public IndexModel(IPageService pageService)
        {
            _pageService = pageService;
        }

        [BindProperty(SupportsGet = true)] public string? searchString { get; set; }
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

            var pages = _pageService.Search(term, skip, pageSize, out var total);
            TotalCount = total;

            Items = pages.Select(p =>
            {
                var title = p.Title ?? string.Empty;
                var bodyPlain = StringUtils.StripHtmlFromString(p.Body ?? string.Empty);
                // Use static method to truncate instead of missing extension
                var snippet = TruncateDotDotDot(bodyPlain, 200);

                if (!string.IsNullOrWhiteSpace(term))
                {
                    title = Highlight(title, term);
                    snippet = Highlight(snippet, term);
                }

                var url = p.Url ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(url) && !url.StartsWith("/"))
                {
                    url = "/Page/" + url;
                }

                return new SearchResultViewModel
                {
                    Id = p.Id,
                    TitleHtml = title,
                    SnippetHtml = snippet,
                    Url = url
                };
            }).ToList();
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