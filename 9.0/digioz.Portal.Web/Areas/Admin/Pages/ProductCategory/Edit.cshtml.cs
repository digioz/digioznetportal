using System;
using digioz.Portal.Dal.Services.Interfaces;
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
            Item = _service.Get(id);
            if (Item == null) return RedirectToPage("/ProductCategory/Index", new { area = "Admin" });
            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid) return Page();
            if (Item == null) return RedirectToPage("/ProductCategory/Index", new { area = "Admin" });
            Item.DateModified = DateTime.UtcNow;
            _service.Update(Item);
            return RedirectToPage("/ProductCategory/Index", new { area = "Admin" });
        }
    }
}
