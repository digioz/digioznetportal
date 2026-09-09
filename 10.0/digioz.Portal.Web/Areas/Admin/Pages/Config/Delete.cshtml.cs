using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;

namespace digioz.Portal.Web.Areas.Admin.Pages.Config
{
    public class DeleteModel : PageModel
    {
        private readonly IConfigService _service;
        private readonly IMemoryCache _cache;
        public DeleteModel(IConfigService service, IMemoryCache cache) { _service = service; _cache = cache; }

        [BindProperty(SupportsGet = true)] public string Id { get; set; } = string.Empty;
        public digioz.Portal.Bo.Config? Item { get; private set; }

        public IActionResult OnGet(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            Item = _service.Get(id);
            if (Item == null) return RedirectToPage("/Config/Index", new { area = "Admin" });
            return Page();
        }

        public IActionResult OnPost()
        {
            if (string.IsNullOrEmpty(Id)) return RedirectToPage("/Config/Index", new { area = "Admin" });
            _service.Delete(Id);
            CacheKeys.InvalidateConfig(_cache);
            return RedirectToPage("/Config/Index", new { area = "Admin" });
        }
    }
}
