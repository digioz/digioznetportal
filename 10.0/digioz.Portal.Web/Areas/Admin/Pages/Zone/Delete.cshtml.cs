using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.Web.Pages.Shared.Components.ZoneMenu;
using Microsoft.Extensions.Caching.Memory;

namespace digioz.Portal.Web.Areas.Admin.Pages.Zone
{
    public class DeleteModel : PageModel
    {
        private readonly IZoneService _zoneService;
        private readonly IMemoryCache _cache;
        public DeleteModel(IZoneService zoneService, IMemoryCache cache) { _zoneService = zoneService; _cache = cache; }

        [BindProperty]
        public int Id { get; set; }

        public IActionResult OnGet(int id)
        {
            Id = id;
            return Page();
        }

        public IActionResult OnPost()
        {
            _zoneService.Delete(Id);
            _cache.Remove(ZoneMenuViewComponent.CacheKey);
            return RedirectToPage("/Zone/Index");
        }
    }
}
