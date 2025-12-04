using System;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.MailingList
{
    public class AddModel : PageModel
    {
        private readonly IMailingListService _service;
        public AddModel(IMailingListService service) { _service = service; }

        [BindProperty] public digioz.Portal.Bo.MailingList Item { get; set; } = new digioz.Portal.Bo.MailingList();

        public void OnGet() { }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid) return Page();
            Item.Id = Guid.NewGuid().ToString();
            
            // Ensure Address is not null for database constraint
            if (string.IsNullOrEmpty(Item.Address))
            {
                Item.Address = string.Empty;
            }
            
            _service.Add(Item);
            return RedirectToPage("/MailingList/Index", new { area = "Admin" });
        }
    }
}
