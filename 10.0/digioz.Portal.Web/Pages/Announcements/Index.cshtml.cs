using System;
using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.Utilities;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Pages.Announcements
{
    public class IndexModel : PageModel
    {
        private readonly IAnnouncementService _announcementService;
        private readonly ICommentsHelper _commentsHelper;

        public IndexModel(IAnnouncementService announcementService, ICommentsHelper commentsHelper)
        {
            _announcementService = announcementService;
            _commentsHelper = commentsHelper;
        }

        public List<Announcement> Items { get; set; } = new List<Announcement>();
        public Dictionary<int, bool> CommentsEnabledMap { get; set; } = new Dictionary<int, bool>();
        
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        public void OnGet(int pageNumber = 1, int pageSize = 10)
        {
            PageNumber = pageNumber > 0 ? Math.Min(pageNumber, 10000) : 1;
            PageSize = pageSize > 0 ? Math.Min(pageSize, 100) : 10;

            // Get paginated visible announcements directly from database
            // This avoids loading all announcements into memory and performs
            // filtering, ordering, and pagination at the database level
            Items = _announcementService.GetPagedVisible(PageNumber, PageSize, out int totalCount);
            TotalCount = totalCount;

            // Use batch method to build comments enabled map
            // This avoids N+1 query issue by fetching all comment configurations at once
            var announcementIds = Items.Select(a => a.Id).ToList();
            CommentsEnabledMap = _commentsHelper.AreCommentsEnabledForAnnouncements(announcementIds);
        }
    }
}
