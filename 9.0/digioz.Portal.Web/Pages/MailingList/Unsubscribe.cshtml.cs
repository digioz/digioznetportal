using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Pages.MailingList
{
    public class UnsubscribeModel : PageModel
    {
        private readonly IMailingListService _mailingListService;
        private readonly IMailingListSubscriberService _subscriberService;
        private readonly IMailingListSubscriberRelationService _relationService;
        private readonly IConfigService _configService;

        public UnsubscribeModel(
            IMailingListService mailingListService,
            IMailingListSubscriberService subscriberService,
            IMailingListSubscriberRelationService relationService,
            IConfigService configService)
        {
            _mailingListService = mailingListService;
            _subscriberService = subscriberService;
            _relationService = relationService;
            _configService = configService;
        }

        public class SubscriptionInfo
        {
            public digioz.Portal.Bo.MailingList MailingList { get; set; } = new digioz.Portal.Bo.MailingList();
            public string RelationId { get; set; } = string.Empty;
        }

        public List<SubscriptionInfo> UserSubscriptions { get; set; } = new List<SubscriptionInfo>();
        public string WebmasterEmail { get; set; } = string.Empty;
        [TempData] public string StatusMessage { get; set; } = string.Empty;
        [TempData] public bool IsSuccess { get; set; }

        public IActionResult OnGet()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return Page(); // Will show login prompt
            }

            LoadSubscriptions();
            LoadConfig();
            return Page();
        }

        public IActionResult OnPostUnsubscribe(string mailingListId)
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity", returnUrl = "/MailingList/Unsubscribe" });
            }

            try
            {
                var userEmail = User.FindFirstValue(ClaimTypes.Email) ?? User.Identity.Name;
                if (string.IsNullOrEmpty(userEmail))
                {
                    StatusMessage = "Unable to determine your email address.";
                    IsSuccess = false;
                    return RedirectToPage();
                }

                var subscriber = _subscriberService.GetAll().FirstOrDefault(s => s.Email == userEmail);
                if (subscriber != null)
                {
                    _relationService.DeleteByMailingListAndSubscriber(mailingListId, subscriber.Id);

                    var mailingList = _mailingListService.Get(mailingListId);
                    StatusMessage = $"Successfully unsubscribed from {mailingList?.Name ?? "the mailing list"}.";
                    IsSuccess = true;
                }
                else
                {
                    StatusMessage = "Subscription not found.";
                    IsSuccess = false;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"An error occurred: {ex.Message}";
                IsSuccess = false;
            }

            return RedirectToPage();
        }

        private void LoadSubscriptions()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email) ?? User.Identity.Name;
            if (string.IsNullOrEmpty(userEmail)) return;

            var subscriber = _subscriberService.GetAll().FirstOrDefault(s => s.Email == userEmail);
            if (subscriber == null) return;

            var relations = _relationService.GetBySubscriberId(subscriber.Id);
            
            UserSubscriptions = relations
                .Select(r => new SubscriptionInfo
                {
                    MailingList = _mailingListService.Get(r.MailingListId),
                    RelationId = r.Id
                })
                .Where(s => s.MailingList != null)
                .OrderBy(s => s.MailingList.Name)
                .ToList();
        }

        private void LoadConfig()
        {
            var configs = _configService.GetAll().ToDictionary(c => c.ConfigKey, c => c.ConfigValue);
            WebmasterEmail = configs.ContainsKey("WebmasterEmail") ? configs["WebmasterEmail"] : "support@example.com";
        }
    }
}