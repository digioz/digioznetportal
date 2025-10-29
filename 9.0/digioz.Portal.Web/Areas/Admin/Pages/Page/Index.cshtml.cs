using System;
using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.Page
{
    public class IndexModel : PageModel
    {
        private readonly IPageService _pageService;
        public IndexModel(IPageService pageService) { _pageService = pageService; }

        public IReadOnlyList<digioz.Portal.Bo.Page> Items { get; private set; } = Array.Empty<digioz.Portal.Bo.Page>();
        [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
        [BindProperty(SupportsGet = true)] public int PageSize { get; set; } = 10;
        public int TotalCount { get; private set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / Math.Max(1, PageSize));

        public void OnGet()
        {
            var all = _pageService.GetAll().OrderByDescending(p => p.Timestamp ?? DateTime.MinValue).ToList();
            TotalCount = all.Count;
            if (PageNumber < 1) PageNumber = 1;
            if (PageSize < 1) PageSize = 10;
            var skip = (PageNumber - 1) * PageSize;
            Items = all.Skip(skip).Take(PageSize).ToList();
        }
    }
}
