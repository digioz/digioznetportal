using System.Collections.Generic;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.Visitor
{
    [Authorize(Roles = "Administrator")]
    public class IndexModel : PageModel
    {
        private readonly IVisitorInfoService _visitorService;
        public IndexModel(IVisitorInfoService visitorService) => _visitorService = visitorService;

        [BindProperty(SupportsGet = true)]
        public string? q { get; set; }

        [BindProperty(SupportsGet = true)]
        public int pageNumber { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int pageSize { get; set; } = 10;

        public IReadOnlyList<digioz.Portal.Bo.VisitorInfo> Items { get; private set; } = new List<digioz.Portal.Bo.VisitorInfo>();
        public int TotalCount { get; private set; }
        public int TotalPages => pageSize > 0 ? (int)System.Math.Ceiling((double)TotalCount / pageSize) : 0;

        public void OnGet()
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            Items = string.IsNullOrWhiteSpace(q)
                ? _visitorService.GetPaged(pageNumber, pageSize)
                : _visitorService.SearchPaged(q!, pageNumber, pageSize);
            TotalCount = string.IsNullOrWhiteSpace(q)
                ? _visitorService.CountAll()
                : _visitorService.CountSearch(q!);
        }
    }
}
