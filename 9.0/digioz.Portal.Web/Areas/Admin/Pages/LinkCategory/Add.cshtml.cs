using System;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.LinkCategory
{
    public class AddModel : PageModel
    {
        private readonly ILinkCategoryService _service;
        public AddModel(ILinkCategoryService service) { _service = service; }

        [BindProperty]
        public digioz.Portal.Bo.LinkCategory Item { get; set; } = new digioz.Portal.Bo.LinkCategory { Visible = true, Timestamp = DateTime.UtcNow };

        public void OnGet() { }

        public IActionResult OnPost()
        {
            if (Item == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid data.");
                return Page();
            }
            if (string.IsNullOrWhiteSpace(Item.Name)) ModelState.AddModelError("Item.Name", "Name is required.");
            if (!ModelState.IsValid) return Page();
            Item.Timestamp = DateTime.UtcNow;
            _service.Add(Item);
            return RedirectToPage("/LinkCategory/Index", new { area = "Admin" });
        }
    }
}
