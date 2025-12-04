using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.MailingList
{
    public class DeleteModel : PageModel
    {
        private readonly IMailingListService _service;
        public DeleteModel(IMailingListService service) { _service = service; }

        [BindProperty(SupportsGet = true)] public string Id { get; set; } = string.Empty;
        public digioz.Portal.Bo.MailingList? Item { get; private set; }

        public IActionResult OnGet(string id)
        {
            Item = _service.Get(id);
            if (Item == null) return RedirectToPage("/MailingList/Index", new { area = "Admin" });
            return Page();
        }

        public IActionResult OnPost()
        {
            _service.Delete(Id);
            return RedirectToPage("/MailingList/Index", new { area = "Admin" });
        }
    }
}
