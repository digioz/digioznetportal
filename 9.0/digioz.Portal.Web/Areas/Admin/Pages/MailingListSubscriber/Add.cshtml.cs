using System;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.MailingListSubscriber
{
    public class AddModel : PageModel
    {
        private readonly IMailingListSubscriberService _service;
        public AddModel(IMailingListSubscriberService service) { _service = service; }

        [BindProperty] public digioz.Portal.Bo.MailingListSubscriber Item { get; set; } = new digioz.Portal.Bo.MailingListSubscriber 
        { 
            Status = true,
            DateCreated = DateTime.UtcNow,
            DateModified = DateTime.UtcNow
        };

        public void OnGet() { }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid) return Page();
            Item.Id = Guid.NewGuid().ToString();
            Item.DateCreated = DateTime.UtcNow;
            Item.DateModified = DateTime.UtcNow;
            _service.Add(Item);
            return RedirectToPage("/MailingListSubscriber/Index", new { area = "Admin" });
        }
    }
}
