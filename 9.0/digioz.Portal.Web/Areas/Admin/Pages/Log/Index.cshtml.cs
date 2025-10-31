using System.Collections.Generic;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.Log
{
    [Authorize(Roles = "Administrator")]
    public class IndexModel : PageModel
    {
        private readonly ILogService _logService;
        public IndexModel(ILogService logService) => _logService = logService;

        [BindProperty(SupportsGet = true)]
        public string? q { get; set; }

        // Rename from 'page' to avoid conflict with Razor Pages 'page' route value
        [BindProperty(SupportsGet = true)]
        public int pageNumber { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int pageSize { get; set; } = 10;

        public IReadOnlyList<digioz.Portal.Bo.Log> Logs { get; private set; } = new List<digioz.Portal.Bo.Log>();
        public int TotalCount { get; private set; }
        public int TotalPages => pageSize > 0 ? (int)System.Math.Ceiling((double)TotalCount / pageSize) : 0;

        public void OnGet()
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            Logs = string.IsNullOrWhiteSpace(q)
                ? _logService.GetPaged(pageNumber, pageSize)
                : _logService.SearchPaged(q!, pageNumber, pageSize);
            TotalCount = string.IsNullOrWhiteSpace(q)
                ? _logService.CountAll()
                : _logService.CountSearch(q!);
        }
    }
}
