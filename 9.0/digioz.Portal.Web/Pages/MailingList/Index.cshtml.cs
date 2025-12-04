using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Pages.MailingList
{
    public class IndexModel : PageModel
    {
        private readonly IMailingListService _mailingListService;
        private readonly IMailingListSubscriberService _subscriberService;
        private readonly IMailingListSubscriberRelationService _relationService;

        public IndexModel(
            IMailingListService mailingListService,
            IMailingListSubscriberService subscriberService,
            IMailingListSubscriberRelationService relationService)
        {
            _mailingListService = mailingListService;
            _subscriberService = subscriberService;
            _relationService = relationService;
        }

        public List<digioz.Portal.Bo.MailingList> MailingLists { get; set; } = new List<digioz.Portal.Bo.MailingList>();
        public HashSet<string> UserSubscriptions { get; set; } = new HashSet<string>();
        [TempData] public string StatusMessage { get; set; } = string.Empty;
        [TempData] public bool IsSuccess { get; set; }

        public void OnGet()
        {
            LoadMailingLists();
            LoadUserSubscriptions();
        }

        public IActionResult OnPostSubscribe(string mailingListId)
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity", returnUrl = "/MailingList/Index" });
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

                // Find or create subscriber
                var subscriber = _subscriberService.GetAll().FirstOrDefault(s => s.Email == userEmail);
                if (subscriber == null)
                {
                    // Create new subscriber
                    subscriber = new digioz.Portal.Bo.MailingListSubscriber
                    {
                        Id = Guid.NewGuid().ToString(),
                        Email = userEmail,
                        FirstName = User.FindFirstValue(ClaimTypes.GivenName) ?? "",
                        LastName = User.FindFirstValue(ClaimTypes.Surname) ?? "",
                        Status = true,
                        DateCreated = DateTime.UtcNow,
                        DateModified = DateTime.UtcNow
                    };
                    _subscriberService.Add(subscriber);
                }
                else if (!subscriber.Status)
                {
                    // Reactivate subscriber
                    subscriber.Status = true;
                    subscriber.DateModified = DateTime.UtcNow;
                    _subscriberService.Update(subscriber);
                }

                // Check if already subscribed
                var existingRelation = _relationService.GetByMailingListAndSubscriber(mailingListId, subscriber.Id);
                if (existingRelation == null)
                {
                    // Create subscription
                    var relation = new digioz.Portal.Bo.MailingListSubscriberRelation
                    {
                        Id = Guid.NewGuid().ToString(),
                        MailingListId = mailingListId,
                        MailingListSubscriberId = subscriber.Id
                    };
                    _relationService.Add(relation);

                    var mailingList = _mailingListService.Get(mailingListId);
                    StatusMessage = $"Successfully subscribed to {mailingList?.Name ?? "the mailing list"}!";
                    IsSuccess = true;
                }
                else
                {
                    StatusMessage = "You are already subscribed to this mailing list.";
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

        public IActionResult OnPostUnsubscribe(string mailingListId)
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity", returnUrl = "/MailingList/Index" });
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
            }
            catch (Exception ex)
            {
                StatusMessage = $"An error occurred: {ex.Message}";
                IsSuccess = false;
            }

            return RedirectToPage();
        }

        private void LoadMailingLists()
        {
            MailingLists = _mailingListService.GetAll().OrderBy(m => m.Name).ToList();
        }

        private void LoadUserSubscriptions()
        {
            if (User.Identity?.IsAuthenticated ?? false)
            {
                var userEmail = User.FindFirstValue(ClaimTypes.Email) ?? User.Identity.Name;
                if (!string.IsNullOrEmpty(userEmail))
                {
                    var subscriber = _subscriberService.GetAll().FirstOrDefault(s => s.Email == userEmail);
                    if (subscriber != null)
                    {
                        var relations = _relationService.GetBySubscriberId(subscriber.Id);
                        UserSubscriptions = relations.Select(r => r.MailingListId).ToHashSet();
                    }
                }
            }
        }
    }
}