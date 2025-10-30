using System;
using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogService _logService;
        private readonly IVisitorInfoService _visitorInfoService;

        public IndexModel(ILogService logService, IVisitorInfoService visitorInfoService)
        {
            _logService = logService;
            _visitorInfoService = visitorInfoService;
        }

        public IReadOnlyList<digioz.Portal.Bo.Log> LatestLogs { get; private set; } = Array.Empty<digioz.Portal.Bo.Log>();
        public IReadOnlyList<digioz.Portal.Bo.VisitorInfo> LatestVisitorLogs { get; private set; } = Array.Empty<digioz.Portal.Bo.VisitorInfo>();

        public void OnGet()
        {
            // Fetch last 5 logs using service method
            LatestLogs = _logService.GetLastN(5, "desc").ToList();

            // Fetch last 5 visitor logs using service method
            LatestVisitorLogs = _visitorInfoService.GetLastN(5, "desc").ToList();
        }
    }
}
