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

        public IndexModel(ILogService logService)
        {
            _logService = logService;
        }

        public IReadOnlyList<digioz.Portal.Bo.Log> LatestLogs { get; private set; } = Array.Empty<digioz.Portal.Bo.Log>();

        public void OnGet()
        {
            // Fetch last5 logs ordered by Timestamp desc
            var all = _logService.GetAll();
            LatestLogs = all
                .OrderByDescending(l => l.Timestamp ?? DateTime.MinValue)
                .Take(5)
                .ToList();
        }
    }
}
