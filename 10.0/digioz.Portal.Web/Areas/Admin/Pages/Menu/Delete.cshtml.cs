using System.Linq;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;

namespace digioz.Portal.Web.Areas.Admin.Pages.Menu
{
    public class DeleteModel : PageModel
    {
        private readonly IMenuService _service;
        private readonly IMemoryCache _cache;
        public DeleteModel(IMenuService service, IMemoryCache cache) { _service = service; _cache = cache; }

        [BindProperty(SupportsGet = true)] public int Id { get; set; }
        public digioz.Portal.Bo.Menu? Item { get; private set; }

        public IActionResult OnGet(int id)
        {
            var existing = _service.Get(id);
            if (existing == null) return NotFound();
            Item = existing;
            Id = id;
            return Page();
        }

        public IActionResult OnPost()
        {
            var existing = _service.Get(Id);
            if (existing == null) return NotFound();
            _service.Delete(Id);
            // Renumber remaining to keep sequence compact
            var all = _service.GetAll().OrderBy(m => m.SortOrder).ToList();
            int order = 1;
            foreach (var m in all)
            {
                if (m.SortOrder != order) { m.SortOrder = order; _service.Update(m); }
                order++;
            }
            _cache.Remove("TopMenu");
            _cache.Remove("LeftMenu");
            return RedirectToPage("/Menu/Index", new { area = "Admin" });
        }
    }
}
