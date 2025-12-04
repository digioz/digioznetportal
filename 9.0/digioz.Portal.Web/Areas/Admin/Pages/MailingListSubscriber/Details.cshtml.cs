using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.MailingListSubscriber
{
    public class DetailsModel : PageModel
    {
        private readonly IMailingListSubscriberService _service;
        public DetailsModel(IMailingListSubscriberService service) { _service = service; }

        public digioz.Portal.Bo.MailingListSubscriber? Item { get; private set; }

        public IActionResult OnGet(string id)
        {
            Item = _service.Get(id);
            if (Item == null) return RedirectToPage("/MailingListSubscriber/Index", new { area = "Admin" });
            return Page();
        }
    }
}
