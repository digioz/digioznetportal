using System;
using System.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace digioz.Portal.Pages.Links
{
    [Authorize]
    public class AddModel : PageModel
    {
        private readonly ILinkService _linkService;
        private readonly ILinkCategoryService _linkCategoryService;

        public AddModel(ILinkService linkService, ILinkCategoryService linkCategoryService)
        {
            _linkService = linkService;
            _linkCategoryService = linkCategoryService;
        }

        [BindProperty]
        public Link Item { get; set; } = new Link();

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
                ModelState.AddModelError("Item.Url", "URL is required.");

            if (Item.LinkCategory <= 0)
                ModelState.AddModelError("Item.LinkCategory", "Category is required.");

            if (!ModelState.IsValid)
            {
                LoadCategories();
                return Page();
            }

            // Set defaults for new user-submitted links
            Item.Timestamp = DateTime.UtcNow;
            Item.Visible = false;  // Not visible until approved
            Item.Approved = false; // Requires admin approval
            Item.Views = 0;

            _linkService.Add(Item);

            TempData["SuccessMessage"] = "Your link has been submitted successfully and is pending approval.";
            return RedirectToPage("/Links/Index");
        }

        private void LoadCategories()
        {
            var categories = _linkCategoryService.GetAll()
                .Where(c => c.Visible)
                .OrderBy(c => c.Name)
                .ToList();
            CategoryList = new SelectList(categories, nameof(LinkCategory.Id), nameof(LinkCategory.Name));
        }
    }
}
