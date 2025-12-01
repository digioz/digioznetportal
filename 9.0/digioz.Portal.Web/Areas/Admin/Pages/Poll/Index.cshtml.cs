using System;
using System.Collections.Generic;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.Poll
{
    public class IndexModel : PageModel
    {
        private readonly IPollService _service;
        public IndexModel(IPollService service) { _service = service; }

        public IReadOnlyList<digioz.Portal.Bo.Poll> Items { get; private set; } = Array.Empty<digioz.Portal.Bo.Poll>();
        [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
        [BindProperty(SupportsGet = true)] public int PageSize { get; set; } = 10;
        public int TotalCount { get; private set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / Math.Max(1, PageSize));

        public void OnGet()
        {
            if (PageNumber < 1) PageNumber = 1;
            if (PageSize < 1) PageSize = 10;
            Items = _service.GetPaged(PageNumber, PageSize, out var total);
            TotalCount = total;
        }
    }
}
