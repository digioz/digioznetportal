using System;
using System.Linq;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Pages.MailingList
{
    public class EmailDisplayModel : PageModel
    {
        private readonly IMailingListCampaignService _campaignService;
        private readonly IConfigService _configService;

        public EmailDisplayModel(
            IMailingListCampaignService campaignService,
            IConfigService configService)
        {
            _campaignService = campaignService;
            _configService = configService;
        }

        public digioz.Portal.Bo.MailingListCampaign? Campaign { get; set; }
        public string SiteName { get; set; } = string.Empty;

        public IActionResult OnGet(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToPage("/Index");
            }

            Campaign = _campaignService.Get(id);
            if (Campaign == null)
            {
                return Page(); // Will show "Campaign Not Found" message
            }

            // Increment visitor count
            Campaign.VisitorCount = Campaign.VisitorCount + 1;
            _campaignService.Update(Campaign);

            // Load site name from config
            var configs = _configService.GetAll().ToDictionary(c => c.ConfigKey, c => c.ConfigValue);
            SiteName = configs.ContainsKey("SiteName") ? configs["SiteName"] : "Our Site";

            return Page();
        }
    }
}