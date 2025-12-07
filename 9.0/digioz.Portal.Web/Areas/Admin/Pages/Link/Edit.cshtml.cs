using System;
using System.Linq;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace digioz.Portal.Web.Areas.Admin.Pages.Link
{
    public class EditModel : PageModel
    {
        private readonly ILinkService _service;
        private readonly ILinkCategoryService _categoryService;
        public EditModel(ILinkService service, ILinkCategoryService categoryService)
        { _service = service; _categoryService = categoryService; }

        [BindProperty] public digioz.Portal.Bo.Link? Item { get; set; }
        public SelectList CategoryList { get; private set; } = new SelectList(Enumerable.Empty<object>());

        public IActionResult OnGet(int id)
        {
            Item = _service.Get(id);
            if (Item == null) return RedirectToPage("/Link/Index", new { area = "Admin" });
            LoadCategories();
            return Page();
        }

        public IActionResult OnPost()
        {
            if (Item == null) return RedirectToPage("/Link/Index", new { area = "Admin" });
            if (string.IsNullOrWhiteSpace(Item.Name)) ModelState.AddModelError("Item.Name", "Name is required.");
            if (string.IsNullOrWhiteSpace(Item.Url)) ModelState.AddModelError("Item.Url", "Url is required.");
            if (Item.LinkCategory <= 0) ModelState.AddModelError("Item.LinkCategory", "Category is required.");
            if (!ModelState.IsValid)
            {
                LoadCategories();
                return Page();
            }

            // Get fresh entity from database to avoid detached entity issues
            var existingLink = _service.Get(Item.Id);
            if (existingLink == null) return RedirectToPage("/Link/Index", new { area = "Admin" });

            // Update properties on the tracked entity
            existingLink.Name = Item.Name;
            existingLink.Url = Item.Url;
            existingLink.Description = Item.Description;
            existingLink.LinkCategory = Item.LinkCategory;
            existingLink.Visible = Item.Visible;
            existingLink.Timestamp = DateTime.UtcNow;

            _service.Update(existingLink);
            return RedirectToPage("/Link/Index", new { area = "Admin" });
        }

        private void LoadCategories()
        {
            var cats = _categoryService.GetAll().OrderBy(c => c.Name).ToList();
            CategoryList = new SelectList(cats, nameof(digioz.Portal.Bo.LinkCategory.Id), nameof(digioz.Portal.Bo.LinkCategory.Name));
        }
    }
}
