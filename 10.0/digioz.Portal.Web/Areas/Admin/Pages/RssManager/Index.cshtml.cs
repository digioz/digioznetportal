using System;
using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.RssManager
{
    public class IndexModel : PageModel
    {
        private readonly IRssService _service;
        public IndexModel(IRssService service) { _service = service; }

        public IReadOnlyList<Rss> Items { get; private set; } = Array.Empty<Rss>();
        [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
        [BindProperty(SupportsGet = true)] public int PageSize { get; set; } = 10;
        public int TotalCount { get; private set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / Math.Max(1, PageSize));

        public void OnGet()
        {
            if (PageNumber < 1) PageNumber = 1;
            if (PageSize < 1 || PageSize > 10) PageSize = 10; // enforce max 10

            Items = _service.GetPage(PageNumber, PageSize, out var total);
            TotalCount = total;
        }
    }
}
