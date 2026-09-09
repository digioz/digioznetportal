using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.MailingListSubscriber
{
    public class DeleteModel : PageModel
    {
        private readonly IMailingListSubscriberService _service;
        public DeleteModel(IMailingListSubscriberService service) { _service = service; }

        [BindProperty(SupportsGet = true)] public string Id { get; set; } = string.Empty;
        public digioz.Portal.Bo.MailingListSubscriber? Item { get; private set; }

        public IActionResult OnGet(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            Item = _service.Get(id);
            return Page();
        }

        public IActionResult OnPost()
        {
            if (string.IsNullOrEmpty(Id)) return RedirectToPage("/MailingListSubscriber/Index", new { area = "Admin" });
            _service.Delete(Id);
            return RedirectToPage("/MailingListSubscriber/Index", new { area = "Admin" });
        }
    }
}
