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
        [BindProperty(SupportsGet = true)] public int pageNumber { get; set; } = 1;
        [BindProperty(SupportsGet = true)] public int pageSize { get; set; } = 10;
        public int TotalCount { get; private set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / Math.Max(1, pageSize));

        public void OnGet()
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            Items = _service.GetPaged(pageNumber, pageSize, out var total);
            TotalCount = total;
        }

        public IActionResult OnPostToggleVisible(string id)
        {
            var poll = _service.Get(id);
            if (poll != null)
            {
                poll.Visible = !(poll.Visible ?? false);
                _service.Update(poll);
            }
            return RedirectToPage(new { pageNumber, pageSize });
        }

        public IActionResult OnPostToggleApproved(string id)
        {
            var poll = _service.Get(id);
            if (poll != null)
            {
                poll.Approved = !(poll.Approved ?? false);
                _service.Update(poll);
            }
            return RedirectToPage(new { pageNumber, pageSize });
        }
    }
}
