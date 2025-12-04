using System.Linq;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;

namespace digioz.Portal.Web.Areas.Admin.Pages.MailingListCampaign
{
    public class EditModel : PageModel
    {
        private readonly IMailingListCampaignService _service;
        private readonly IMailingListService _mailingListService;
        private readonly IMailingListCampaignRelationService _relationService;

        public EditModel(
            IMailingListCampaignService service,
            IMailingListService mailingListService,
            IMailingListCampaignRelationService relationService)
        {
            _service = service;
            _mailingListService = mailingListService;
            _relationService = relationService;
        }

        [BindProperty] public digioz.Portal.Bo.MailingListCampaign? Item { get; set; }
        [BindProperty] public string SelectedMailingListId { get; set; } = string.Empty;
        public SelectList MailingLists { get; set; } = new SelectList(Enumerable.Empty<object>());

        public IActionResult OnGet(string id)
        {
            Item = _service.Get(id);
            if (Item == null) return RedirectToPage("/MailingListCampaign/Index", new { area = "Admin" });

            // Load current mailing list relation
            var relation = _relationService.GetByCampaignId(id).FirstOrDefault();
            if (relation != null)
            {
                SelectedMailingListId = relation.MailingListId;
            }

            LoadMailingLists();
            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                LoadMailingLists();
                return Page();
            }

            if (Item == null) return RedirectToPage("/MailingListCampaign/Index", new { area = "Admin" });

            _service.Update(Item);

            // Update campaign-mailinglist relation
            var existingRelations = _relationService.GetByCampaignId(Item.Id);

            // Remove all existing relations for this campaign
            foreach (var rel in existingRelations)
            {
                _relationService.Delete(rel.Id);
            }

            // Add new relation
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
            MailingLists = new SelectList(lists, "Id", "Name", SelectedMailingListId);
        }
    }
}
