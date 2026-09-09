using System;
using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.Announcement
{
    public class IndexModel : PageModel
    {
        private readonly IAnnouncementService _announcementService;
        public IndexModel(IAnnouncementService announcementService) { _announcementService = announcementService; }

        public IReadOnlyList<digioz.Portal.Bo.Announcement> Items { get; private set; } = Array.Empty<digioz.Portal.Bo.Announcement>();
        [BindProperty(SupportsGet = true)] public int pageNumber { get; set; } = 1;
        [BindProperty(SupportsGet = true)] public int pageSize { get; set; } = 10;
        public int TotalCount { get; private set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / Math.Max(1, pageSize));

        public void OnGet()
        {
            var all = _announcementService.GetAll().OrderByDescending(p => p.Timestamp ?? DateTime.MinValue).ToList();
            TotalCount = all.Count;
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            var skip = (pageNumber - 1) * pageSize;
            Items = all.Skip(skip).Take(pageSize).ToList();
        }
    }
}
