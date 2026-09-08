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
            // Get the number of announcements to display from config using targeted query
            var numberOfAnnouncementsConfig = _configService.GetByKey("NumberOfAnnouncements");
            
            int numberOfAnnouncements = 3; // Default value
            if (numberOfAnnouncementsConfig != null && int.TryParse(numberOfAnnouncementsConfig.ConfigValue, out var configValue))
            {
                numberOfAnnouncements = configValue;
            }

            // Get only the required number of visible announcements directly from database
            // This avoids loading all announcements into memory
            var announcementsToDisplay = _announcementService.GetVisible(numberOfAnnouncements);

            if (!announcementsToDisplay.Any())
            {
                return Task.FromResult<IViewComponentResult>(View(new List<Announcement>()));
            }

            // Use batch method to check comments enabled for all announcements at once
            // This avoids N+1 query issue by fetching configuration data once
            var announcementIds = announcementsToDisplay.Select(a => a.Id).ToList();
            var commentsEnabledMap = _commentsHelper.AreCommentsEnabledForAnnouncements(announcementIds);

            ViewBag.CommentsEnabledMap = commentsEnabledMap;

            return Task.FromResult<IViewComponentResult>(View(announcementsToDisplay));
        }
    }
}
