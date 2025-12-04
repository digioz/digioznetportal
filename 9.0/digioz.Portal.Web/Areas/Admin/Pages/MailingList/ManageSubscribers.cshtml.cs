using System;
using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.MailingList
{
    public class ManageSubscribersModel : PageModel
    {
        private readonly IMailingListService _mailingListService;
        private readonly IMailingListSubscriberService _subscriberService;
        private readonly IMailingListSubscriberRelationService _relationService;

        public ManageSubscribersModel(
            IMailingListService mailingListService,
            IMailingListSubscriberService subscriberService,
            IMailingListSubscriberRelationService relationService)
        {
            _mailingListService = mailingListService;
            _subscriberService = subscriberService;
            _relationService = relationService;
        }

        public string MailingListId { get; set; } = string.Empty;
        public string MailingListName { get; set; } = string.Empty;
        public List<digioz.Portal.Bo.MailingListSubscriber> AvailableSubscribers { get; set; } = new List<digioz.Portal.Bo.MailingListSubscriber>();
        public List<digioz.Portal.Bo.MailingListSubscriber> SubscribedMembers { get; set; } = new List<digioz.Portal.Bo.MailingListSubscriber>();

        public IActionResult OnGet(string id)
        {
            var mailingList = _mailingListService.Get(id);
            if (mailingList == null) return RedirectToPage("/MailingList/Index", new { area = "Admin" });

            MailingListId = id;
            MailingListName = mailingList.Name;

            LoadSubscribers();
            return Page();
        }

        public IActionResult OnPostAddSubscriber(string mailingListId, string subscriberId)
        {
            // Check if relation already exists
            var existing = _relationService.GetByMailingListAndSubscriber(mailingListId, subscriberId);
            if (existing == null)
            {
                var relation = new digioz.Portal.Bo.MailingListSubscriberRelation
                {
                    Id = Guid.NewGuid().ToString(),
                    MailingListId = mailingListId,
                    MailingListSubscriberId = subscriberId
                };
                _relationService.Add(relation);
            }

            return RedirectToPage(new { id = mailingListId });
        }

        public IActionResult OnPostRemoveSubscriber(string mailingListId, string subscriberId)
        {
            _relationService.DeleteByMailingListAndSubscriber(mailingListId, subscriberId);
            return RedirectToPage(new { id = mailingListId });
        }

        private void LoadSubscribers()
        {
            var allSubscribers = _subscriberService.GetAll();
            var relations = _relationService.GetByMailingListId(MailingListId);
            var subscribedIds = relations.Select(r => r.MailingListSubscriberId).ToList();

            SubscribedMembers = allSubscribers.Where(s => subscribedIds.Contains(s.Id)).OrderBy(s => s.Email).ToList();
            AvailableSubscribers = allSubscribers.Where(s => !subscribedIds.Contains(s.Id)).OrderBy(s => s.Email).ToList();
        }
    }
}
