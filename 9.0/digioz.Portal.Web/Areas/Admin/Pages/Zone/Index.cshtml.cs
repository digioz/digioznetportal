using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Dal.Services;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ZoneEntity = digioz.Portal.Bo.Zone;

namespace digioz.Portal.Web.Areas.Admin.Pages.Zone
{
    public class IndexModel : PageModel
    {
        private readonly IZoneService _zoneService;
        public IndexModel(IZoneService zoneService)
        {
            _zoneService = zoneService;
        }

        public IList<ZoneEntity> Zones { get; private set; } = new List<ZoneEntity>();
        [BindProperty(SupportsGet = true)] public int pageNumber { get; set; } = 1;
        [BindProperty(SupportsGet = true)] public int pageSize { get; set; } = 10;
        public int TotalCount { get; private set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / Math.Max(1, pageSize));

        public void OnGet([FromQuery]int page = 1)
        {
            var all = _zoneService.GetAll().OrderBy(z => z.Location).ThenBy(z => z.Name).ToList();
            TotalCount = all.Count;
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            var skip = (pageNumber - 1) * pageSize;
            Zones = all.Skip(skip).Take(pageSize).ToList();
        }
    }
}
