using System;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.LinkCategory
{
    public class EditModel : PageModel
    {
        private readonly ILinkCategoryService _service;
        public EditModel(ILinkCategoryService service) { _service = service; }

        [BindProperty] public digioz.Portal.Bo.LinkCategory? Item { get; set; }

        public IActionResult OnGet(int id)
        {
            Item = _service.Get(id);
            if (Item == null) return RedirectToPage("/LinkCategory/Index", new { area = "Admin" });
            return Page();
        }

        public IActionResult OnPost()
        {
            if (Item == null) return RedirectToPage("/LinkCategory/Index", new { area = "Admin" });
            if (string.IsNullOrWhiteSpace(Item.Name)) ModelState.AddModelError("Item.Name", "Name is required.");
            if (!ModelState.IsValid) return Page();
            Item.Timestamp = DateTime.UtcNow;
            _service.Update(Item);
            return RedirectToPage("/LinkCategory/Index", new { area = "Admin" });
        }
    }
}
