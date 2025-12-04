using System;
using System.Linq;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace digioz.Portal.Web.Areas.Admin.Pages.MailingListCampaign
{
    public class AddModel : PageModel
    {
        private readonly IMailingListCampaignService _service;
        private readonly IMailingListService _mailingListService;
        private readonly IMailingListCampaignRelationService _relationService;

        public AddModel(
            IMailingListCampaignService service,
            IMailingListService mailingListService,
            IMailingListCampaignRelationService relationService)
        {
            _service = service;
            _mailingListService = mailingListService;
            _relationService = relationService;
        }

        [BindProperty] public digioz.Portal.Bo.MailingListCampaign Item { get; set; } = new digioz.Portal.Bo.MailingListCampaign { DateCreated = DateTime.UtcNow };
        [BindProperty] public string SelectedMailingListId { get; set; } = string.Empty;
        public SelectList MailingLists { get; set; } = new SelectList(Enumerable.Empty<object>());

        public void OnGet()
        {
            LoadMailingLists();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                LoadMailingLists();
                return Page();
            }

            Item.Id = Guid.NewGuid().ToString();
            Item.DateCreated = DateTime.UtcNow;
            _service.Add(Item);

            // Create campaign-mailinglist relation
            if (!string.IsNullOrEmpty(SelectedMailingListId))
            {
                var relation = new digioz.Portal.Bo.MailingListCampaignRelation
                {
                    Id = Guid.NewGuid().ToString(),
                    MailingListId = SelectedMailingListId,
                    MailingListCampaignId = Item.Id
                };
                _relationService.Add(relation);
            }

            return RedirectToPage("/MailingListCampaign/Index", new { area = "Admin" });
        }

        private void LoadMailingLists()
        {
            var lists = _mailingListService.GetAll().OrderBy(m => m.Name);
            MailingLists = new SelectList(lists, "Id", "Name");
        }
    }
}
