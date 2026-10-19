using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.MailingListCampaign
{
    public class DeleteModel : PageModel
    {
        private readonly IMailingListCampaignService _service;
        public DeleteModel(IMailingListCampaignService service) { _service = service; }

        [BindProperty(SupportsGet = true)] public string Id { get; set; } = string.Empty;
        public digioz.Portal.Bo.MailingListCampaign? Item { get; private set; }

        public IActionResult OnGet(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            Item = _service.Get(id);
            if (Item == null) return RedirectToPage("/MailingListCampaign/Index", new { area = "Admin" });
            return Page();
        }

        public IActionResult OnPost()
        {
            if (string.IsNullOrEmpty(Id)) return RedirectToPage("/MailingListCampaign/Index", new { area = "Admin" });
            _service.Delete(Id);
            return RedirectToPage("/MailingListCampaign/Index", new { area = "Admin" });
        }
    }
}
