using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;

namespace digioz.Portal.Web.Pages.Shared.Components.ZoneMenu
{
    public class ZoneMenuViewComponent : ViewComponent
    {
        private readonly IZoneService _zoneService;
        public ZoneMenuViewComponent(IZoneService zoneService)
        {
            _zoneService = zoneService;
        }

        public Task<IViewComponentResult> InvokeAsync(string location)
        {
            if (string.IsNullOrWhiteSpace(location))
            {
                // Nothing to render
                return Task.FromResult<IViewComponentResult>(Content(string.Empty));
            }

            // Fetch and filter zones by location (case-insensitive) and visibility
            List<Zone> zones = _zoneService.GetAll()
                .Where(z => z.Location != null && z.Location.Equals(location, System.StringComparison.OrdinalIgnoreCase) && z.Visible)
                .OrderBy(z => z.Name)
                .ToList();

            ViewBag.Location = location;
            return Task.FromResult<IViewComponentResult>(View(zones));
        }
    }
}
