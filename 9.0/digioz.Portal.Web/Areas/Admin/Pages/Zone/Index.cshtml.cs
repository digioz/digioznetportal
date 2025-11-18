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
        [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
        [BindProperty(SupportsGet = true)] public int PageSize { get; set; } = 10;
        public int TotalCount { get; private set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / Math.Max(1, PageSize));

        public void OnGet([FromQuery]int page = 1)
        {
            var all = _zoneService.GetAll().OrderBy(z => z.Location).ThenBy(z => z.Name).ToList();
            TotalCount = all.Count;
            if (PageNumber < 1) PageNumber = 1;
            if (PageSize < 1) PageSize = 10;
            var skip = (PageNumber - 1) * PageSize;
            Zones = all.Skip(skip).Take(PageSize).ToList();
        }
    }
}
