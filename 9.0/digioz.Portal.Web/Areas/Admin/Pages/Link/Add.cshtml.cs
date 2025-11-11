using System;
using System.Linq;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace digioz.Portal.Web.Areas.Admin.Pages.Link
{
    public class AddModel : PageModel
    {
        private readonly ILinkService _service;
        private readonly ILinkCategoryService _categoryService;
        public AddModel(ILinkService service, ILinkCategoryService categoryService)
        {
            _service = service;
            _categoryService = categoryService;
        }

        [BindProperty]
        public digioz.Portal.Bo.Link Item { get; set; } = new digioz.Portal.Bo.Link { Visible = true, Timestamp = DateTime.UtcNow };

        public SelectList CategoryList { get; private set; } = new SelectList(Enumerable.Empty<object>());

        public void OnGet()
        {
            LoadCategories();
        }

        public IActionResult OnPost()
        {
            if (Item == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid link data.");
                LoadCategories();
                return Page();
            }

            if (string.IsNullOrWhiteSpace(Item.Name))
                ModelState.AddModelError("Item.Name", "Name is required.");

            if (string.IsNullOrWhiteSpace(Item.Url))
                ModelState.AddModelError("Item.Url", "Url is required.");

            if (Item.LinkCategory <= 0)
                ModelState.AddModelError("Item.LinkCategory", "Category is required.");

            if (!ModelState.IsValid)
            {
                LoadCategories();
                return Page();
            }

            Item.Timestamp = DateTime.UtcNow;
            _service.Add(Item);
            return RedirectToPage("/Link/Index", new { area = "Admin" });
        }

        private void LoadCategories()
        {
            var cats = _categoryService.GetAll().OrderBy(c => c.Name).ToList();
            CategoryList = new SelectList(cats, nameof(digioz.Portal.Bo.LinkCategory.Id), nameof(digioz.Portal.Bo.LinkCategory.Name));
        }
    }
}
