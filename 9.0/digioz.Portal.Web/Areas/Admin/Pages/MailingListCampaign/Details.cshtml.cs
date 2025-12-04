using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.MailingListCampaign
{
    public class DetailsModel : PageModel
    {
        private readonly IMailingListCampaignService _service;
        public DetailsModel(IMailingListCampaignService service) { _service = service; }

        public digioz.Portal.Bo.MailingListCampaign? Item { get; private set; }

        public IActionResult OnGet(string id)
        {
            Item = _service.Get(id);
            if (Item == null) return RedirectToPage("/MailingListCampaign/Index", new { area = "Admin" });
            return Page();
        }
    }
}
