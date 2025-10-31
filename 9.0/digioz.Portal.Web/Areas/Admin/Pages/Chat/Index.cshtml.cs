using System;
using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.Chat
{
    public class IndexModel : PageModel
    {
        private readonly IChatService _service;
        public IndexModel(IChatService service) { _service = service; }

        public IReadOnlyList<digioz.Portal.Bo.Chat> Items { get; private set; } = Array.Empty<digioz.Portal.Bo.Chat>();
        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;
        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; private set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / Math.Max(1, PageSize));

        public void OnGet()
        {
            var all = _service.GetAll()
                .OrderByDescending(c => c.Timestamp ?? DateTime.MinValue)
                .ThenByDescending(c => c.Id)
                .ToList();
            TotalCount = all.Count;
            if (PageNumber < 1) PageNumber = 1;
            if (PageSize < 1) PageSize = 10;
            var skip = (PageNumber - 1) * PageSize;
            Items = all.Skip(skip).Take(PageSize).ToList();
        }
    }
}
