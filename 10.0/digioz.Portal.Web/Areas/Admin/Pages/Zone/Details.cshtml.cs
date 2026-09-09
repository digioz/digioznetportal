using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using digioz.Portal.Dal.Services.Interfaces;
using ZoneEntity = digioz.Portal.Bo.Zone;

namespace digioz.Portal.Web.Areas.Admin.Pages.Zone
{
    public class DetailsModel : PageModel
    {
        private readonly IZoneService _zoneService;
        public DetailsModel(IZoneService zoneService) { _zoneService = zoneService; }

        public ZoneEntity? Item { get; private set; }

        public IActionResult OnGet(int id)
        {
            Item = _zoneService.Get(id);
            if (Item == null) return RedirectToPage("/Zone/Index");
            return Page();
        }
    }
}
