using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;

namespace digioz.Portal.Web.Pages.Shared.Components.ZoneMenu
{
    public class ZoneMenuViewComponent : ViewComponent
    {
        public const string CacheKey = "ZoneMenu_VisibleZones";
        private readonly IZoneService _zoneService;
        private readonly IMemoryCache _cache;

        public ZoneMenuViewComponent(IZoneService zoneService, IMemoryCache cache)
        {
            _zoneService = zoneService;
            _cache = cache;
        }

        public Task<IViewComponentResult> InvokeAsync(string location)
        {
            if (string.IsNullOrWhiteSpace(location))
            {
                // Nothing to render
                return Task.FromResult<IViewComponentResult>(Content(string.Empty));
            }

            // All visible zones are loaded once per cache window; the layout invokes this
            // component several times per page for different locations.
            if (!_cache.TryGetValue(CacheKey, out List<Zone>? allZones) || allZones == null)
            {
                allZones = _zoneService.GetAll()
                    .Where(z => z.Visible && z.Location != null)
                    .OrderBy(z => z.Name)
                    .ToList();
                _cache.Set(CacheKey, allZones, new MemoryCacheEntryOptions { SlidingExpiration = TimeSpan.FromMinutes(15) });
            }

            List<Zone> zones = allZones
                .Where(z => z.Location!.Equals(location, StringComparison.OrdinalIgnoreCase))
                .ToList();

            ViewBag.Location = location;
            return Task.FromResult<IViewComponentResult>(View(zones));
        }
    }
}
