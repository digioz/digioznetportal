using System;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.ProductCategory
{
    public class AddModel : PageModel
    {
        private readonly IProductCategoryService _service;
        public AddModel(IProductCategoryService service) { _service = service; }

        [BindProperty] public Bo.ProductCategory Item { get; set; } = new Bo.ProductCategory { Visible = true };

        public void OnGet() { }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid) return Page();
            Item.Name = InputSanitizer.SanitizeText(Item.Name);
            Item.Description = InputSanitizer.SanitizeText(Item.Description);
            if (string.IsNullOrEmpty(Item.Id))
            {
                Item.Id = Guid.NewGuid().ToString();
            }
            Item.DateCreated = DateTime.UtcNow;
            Item.DateModified = DateTime.UtcNow;
            _service.Add(Item);
            return RedirectToPage("/ProductCategory/Index", new { area = "Admin" });
        }
    }
}
