using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace digioz.Portal.Web.Pages.Shared.Components.Announcements
{
    public class AnnouncementsViewComponent : ViewComponent
    {
        private readonly IAnnouncementService _announcementService;
        private readonly IConfigService _configService;
        private readonly ICommentsHelper _commentsHelper;

        public AnnouncementsViewComponent(
            IAnnouncementService announcementService, 
            IConfigService configService,
            ICommentsHelper commentsHelper)
        {
            _announcementService = announcementService;
            _configService = configService;
            _commentsHelper = commentsHelper;
        }

        public Task<IViewComponentResult> InvokeAsync()
        {
            // Get all visible announcements ordered by timestamp descending
            var allAnnouncements = _announcementService.GetAll()
                .Where(a => a.Visible)
                .OrderByDescending(a => a.Timestamp)
                .ToList();

            if (!allAnnouncements.Any())
            {
                return Task.FromResult<IViewComponentResult>(View(new List<Announcement>()));
            }

            // Get the number of announcements to display from config
            var numberOfAnnouncementsConfig = _configService.GetAll()
                .FirstOrDefault(c => c.ConfigKey == "NumberOfAnnouncements");
            
            int numberOfAnnouncements = 3; // Default value
            if (numberOfAnnouncementsConfig != null && int.TryParse(numberOfAnnouncementsConfig.ConfigValue, out var configValue))
            {
                numberOfAnnouncements = configValue;
            }

            var announcementsToDisplay = allAnnouncements.Take(numberOfAnnouncements).ToList();

            // Build a dictionary of announcement ID -> comments enabled
            var commentsEnabledMap = new Dictionary<int, bool>();
            foreach (var announcement in announcementsToDisplay)
            {
                commentsEnabledMap[announcement.Id] = _commentsHelper.IsCommentsEnabledForAnnouncement(announcement.Id);
            }

            ViewBag.CommentsEnabledMap = commentsEnabledMap;

            return Task.FromResult<IViewComponentResult>(View(announcementsToDisplay));
        }
    }
}
