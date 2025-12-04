using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.MailingList
{
    public class EditModel : PageModel
    {
        private readonly IMailingListService _service;
        public EditModel(IMailingListService service) { _service = service; }

        [BindProperty] public digioz.Portal.Bo.MailingList? Item { get; set; }

        public IActionResult OnGet(string id)
        {
            Item = _service.Get(id);
            if (Item == null) return RedirectToPage("/MailingList/Index", new { area = "Admin" });
            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid) return Page();
            if (Item == null) return RedirectToPage("/MailingList/Index", new { area = "Admin" });
            
            // Ensure Address is not null for database constraint
            if (string.IsNullOrEmpty(Item.Address))
            {
                Item.Address = string.Empty;
            }
            
            _service.Update(Item);
            return RedirectToPage("/MailingList/Index", new { area = "Admin" });
        }
    }
}
