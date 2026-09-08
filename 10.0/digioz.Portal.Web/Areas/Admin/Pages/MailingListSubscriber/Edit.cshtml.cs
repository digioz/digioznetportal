using System;
using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.MailingListSubscriber
{
    public class EditModel : PageModel
    {
        private readonly IMailingListSubscriberService _service;
        public EditModel(IMailingListSubscriberService service) { _service = service; }

        [BindProperty] public digioz.Portal.Bo.MailingListSubscriber? Item { get; set; }

        public IActionResult OnGet(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            Item = _service.Get(id);
            if (Item == null) return RedirectToPage("/MailingListSubscriber/Index", new { area = "Admin" });
            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid) return Page();
            if (Item == null) return RedirectToPage("/MailingListSubscriber/Index", new { area = "Admin" });
            Item.FirstName = InputSanitizer.SanitizeText(Item.FirstName);
            Item.LastName = InputSanitizer.SanitizeText(Item.LastName);
            Item.DateModified = DateTime.UtcNow;
            _service.Update(Item);
            return RedirectToPage("/MailingListSubscriber/Index", new { area = "Admin" });
        }
    }
}
