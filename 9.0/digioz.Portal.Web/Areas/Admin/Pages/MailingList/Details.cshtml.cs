using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.MailingList
{
    public class DetailsModel : PageModel
    {
        private readonly IMailingListService _service;
        public DetailsModel(IMailingListService service) { _service = service; }

        public digioz.Portal.Bo.MailingList? Item { get; private set; }

        public IActionResult OnGet(string id)
        {
            Item = _service.Get(id);
            if (Item == null) return RedirectToPage("/MailingList/Index", new { area = "Admin" });
            return Page();
        }
    }
}
