using System;
using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;

namespace digioz.Portal.Web.Areas.Admin.Pages.Menu
{
    public class IndexModel : PageModel
    {
        private readonly IMenuService _service;
        private readonly IMemoryCache _cache;
        public IndexModel(IMenuService service, IMemoryCache cache) { _service = service; _cache = cache; }

        public IReadOnlyList<digioz.Portal.Bo.Menu> Items { get; private set; } = Array.Empty<digioz.Portal.Bo.Menu>();
        [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
        [BindProperty(SupportsGet = true)] public int PageSize { get; set; } = 10;
        public int TotalCount { get; private set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / Math.Max(1, PageSize));

        public void OnGet()
        {
            var all = _service.GetAll().OrderBy(m => m.SortOrder).ToList();
            TotalCount = all.Count;
            if (PageNumber < 1) PageNumber = 1;
            if (PageSize < 1) PageSize = 10;
            var skip = (PageNumber - 1) * PageSize;
            Items = all.Skip(skip).Take(PageSize).ToList();
        }

        public class ReorderRequest
        {
            public List<int> OrderedIds { get; set; } = new();
            public int PageNumber { get; set; }
            public int PageSize { get; set; }
        }

        [ValidateAntiForgeryToken]
        public IActionResult OnPostReorder([FromBody] ReorderRequest request)
        {
            if (request == null || request.OrderedIds == null || request.OrderedIds.Count == 0)
            {
                return BadRequest(new { ok = false, message = "Invalid payload" });
            }

            var all = _service.GetAll().OrderBy(m => m.SortOrder).ToList();
            var total = all.Count;
            var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
            var pageSize = request.PageSize < 1 ? 10 : request.PageSize;
            var skip = (pageNumber - 1) * pageSize;
            if (skip >= total) skip = Math.Max(0, total - pageSize);
            var take = Math.Min(pageSize, total - skip);

            if (request.OrderedIds.Count != take)
            {
                return BadRequest(new { ok = false, message = "Payload size mismatch" });
            }

            // Build new global order by replacing the page segment with the provided order
            var currentOrderIds = all.Select(m => m.Id).ToList();
            currentOrderIds.RemoveRange(skip, take);
            currentOrderIds.InsertRange(skip, request.OrderedIds);

            // Re-number sort orders sequentially
            var idToSort = new Dictionary<int, int>(capacity: currentOrderIds.Count);
            for (int i = 0; i < currentOrderIds.Count; i++)
            {
                idToSort[currentOrderIds[i]] = i + 1; //1-based
            }

            bool anyChanged = false;
            foreach (var menu in all)
            {
                var newOrder = idToSort[menu.Id];
                if (menu.SortOrder != newOrder)
                {
                    menu.SortOrder = newOrder;
                    _service.Update(menu);
                    anyChanged = true;
                }
            }
            if (anyChanged)
            {
                _cache.Remove("TopMenu");
                _cache.Remove("LeftMenu");
            }

            return new JsonResult(new { ok = true, changed = anyChanged });
        }
    }
}
