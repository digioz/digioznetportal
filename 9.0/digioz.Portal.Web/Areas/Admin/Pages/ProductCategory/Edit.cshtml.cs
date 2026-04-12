using System;
using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.ProductCategory
{
    public class EditModel : PageModel
    {
        private readonly IProductCategoryService _service;
        public EditModel(IProductCategoryService service) { _service = service; }

        [BindProperty] public Bo.ProductCategory? Item { get; set; }

        public IActionResult OnGet(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            Item = _service.Get(id);
            if (Item == null) return RedirectToPage("/ProductCategory/Index", new { area = "Admin" });
            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid) return Page();
            if (Item == null) return RedirectToPage("/ProductCategory/Index", new { area = "Admin" });
            Item.Name = InputSanitizer.SanitizeText(Item.Name);
            Item.Description = InputSanitizer.SanitizeText(Item.Description);
            Item.DateModified = DateTime.UtcNow;
            _service.Update(Item);
            return RedirectToPage("/ProductCategory/Index", new { area = "Admin" });
        }
    }
}
