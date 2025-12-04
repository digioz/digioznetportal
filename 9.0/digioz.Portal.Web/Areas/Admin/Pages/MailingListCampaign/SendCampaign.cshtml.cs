using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.MailingListCampaign
{
    public class SendCampaignModel : PageModel
    {
        private readonly IMailingListCampaignService _campaignService;
        private readonly IMailingListCampaignRelationService _campaignRelationService;
        private readonly IMailingListService _mailingListService;
        private readonly IMailingListSubscriberRelationService _subscriberRelationService;
        private readonly IMailingListSubscriberService _subscriberService;
        private readonly IEmailNotificationService _emailService;
        private readonly IConfigService _configService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public SendCampaignModel(
            IMailingListCampaignService campaignService,
            IMailingListCampaignRelationService campaignRelationService,
            IMailingListService mailingListService,
            IMailingListSubscriberRelationService subscriberRelationService,
            IMailingListSubscriberService subscriberService,
            IEmailNotificationService emailService,
            IConfigService configService,
            IWebHostEnvironment webHostEnvironment)
        {
            _campaignService = campaignService;
            _campaignRelationService = campaignRelationService;
            _mailingListService = mailingListService;
            _subscriberRelationService = subscriberRelationService;
            _subscriberService = subscriberService;
            _emailService = emailService;
            _configService = configService;
            _webHostEnvironment = webHostEnvironment;
        }

        public digioz.Portal.Bo.MailingListCampaign Campaign { get; set; } = new digioz.Portal.Bo.MailingListCampaign();
        public string MailingListName { get; set; } = string.Empty;
        public int SubscriberCount { get; set; }
        [TempData] public string StatusMessage { get; set; } = string.Empty;
        [TempData] public bool IsSuccess { get; set; }

        public IActionResult OnGet(string id)
        {
            Campaign = _campaignService.Get(id);
            if (Campaign == null) return RedirectToPage("/MailingListCampaign/Index", new { area = "Admin" });

            LoadCampaignInfo(id);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            Campaign = _campaignService.Get(id);
            if (Campaign == null) return RedirectToPage("/MailingListCampaign/Index", new { area = "Admin" });

            LoadCampaignInfo(id);

            if (SubscriberCount == 0)
            {
                StatusMessage = "No active subscribers found. Cannot send campaign.";
                IsSuccess = false;
                return Page();
            }

            try
            {
                // Get all configs
                var configs = _configService.GetAll().ToDictionary(c => c.ConfigKey, c => c.ConfigValue);
                string siteUrl = configs.ContainsKey("SiteURL") ? configs["SiteURL"] : "";
                string siteName = configs.ContainsKey("SiteName") ? configs["SiteName"] : "";

                // Load email template
                string templatePath = Path.Combine(_webHostEnvironment.WebRootPath, "img", "Emails", "EmailNotification.htm");
                string emailTemplate = System.IO.File.ReadAllText(templatePath);

                // Get campaign mailing list
                var campaignRelation = _campaignRelationService.GetByCampaignId(id).FirstOrDefault();
                if (campaignRelation == null)
                {
                    StatusMessage = "Campaign is not assigned to any mailing list.";
                    IsSuccess = false;
                    return Page();
                }

                var mailingList = _mailingListService.Get(campaignRelation.MailingListId);

                // Get all active subscribers
                var subscriberRelations = _subscriberRelationService.GetByMailingListId(campaignRelation.MailingListId);
                var subscriberIds = subscriberRelations.Select(r => r.MailingListSubscriberId).ToList();
                var activeSubscribers = _subscriberService.GetAll()
                    .Where(s => subscriberIds.Contains(s.Id) && s.Status)
                    .ToList();

                int successCount = 0;
                int failCount = 0;

                // Send to each subscriber
                foreach (var subscriber in activeSubscribers)
                {
                    try
                    {
                        // Prepare email body
                        string emailBody = emailTemplate
                            .Replace("#SITENAME#", siteName)
                            .Replace("#SITEURL#", siteUrl)
                            .Replace("#TO#", $"Dear {subscriber.FirstName} {subscriber.LastName},")
                            .Replace("#CONTENT#", Campaign.Body);

                        // Determine From address
                        string fromEmail = !string.IsNullOrEmpty(Campaign.FromEmail) 
                            ? Campaign.FromEmail 
                            : (!string.IsNullOrEmpty(mailingList.DefaultEmailFrom) 
                                ? mailingList.DefaultEmailFrom 
                                : configs.ContainsKey("WebmasterEmail") ? configs["WebmasterEmail"] : "");

                        // Send email
                        bool sent = await _emailService.SendEmailAsync(
                            subscriber.Email,
                            Campaign.Subject,
                            emailBody
                        );

                        if (sent)
                            successCount++;
                        else
                            failCount++;
                    }
                    catch
                    {
                        failCount++;
                    }
                }

                StatusMessage = $"Campaign sent! Success: {successCount}, Failed: {failCount}";
                IsSuccess = successCount > 0;
                return RedirectToPage(new { id });
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error sending campaign: {ex.Message}";
                IsSuccess = false;
                return Page();
            }
        }

        private void LoadCampaignInfo(string campaignId)
        {
            var campaignRelation = _campaignRelationService.GetByCampaignId(campaignId).FirstOrDefault();
            if (campaignRelation != null)
            {
                var mailingList = _mailingListService.Get(campaignRelation.MailingListId);
                if (mailingList != null)
                {
                    MailingListName = mailingList.Name;

                    // Count active subscribers
                    var subscriberRelations = _subscriberRelationService.GetByMailingListId(campaignRelation.MailingListId);
                    var subscriberIds = subscriberRelations.Select(r => r.MailingListSubscriberId).ToList();
                    SubscriberCount = _subscriberService.GetAll()
                        .Count(s => subscriberIds.Contains(s.Id) && s.Status);
                }
            }
        }
    }
}
