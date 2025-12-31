using System;
using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace digioz.Portal.Web.Areas.Admin.Pages.Link
{
    public class IndexModel : PageModel
    {
        private readonly ILinkService _service;
        private readonly ILinkCategoryService _categoryService;
        public IndexModel(ILinkService service, ILinkCategoryService categoryService)
        {
            _service = service;
            _categoryService = categoryService;
        }

        public IReadOnlyList<digioz.Portal.Bo.Link> Items { get; private set; } = Array.Empty<digioz.Portal.Bo.Link>();
        public Dictionary<int, string> CategoryNames { get; private set; } = new();
        public List<SelectListItem> CategoryFilterOptions { get; private set; } = new();
        [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } =1;
        [BindProperty(SupportsGet = true)] public int PageSize { get; set; } =10;
        [BindProperty(SupportsGet = true)] public string VisibilityFilter { get; set; } = "all";
        [BindProperty(SupportsGet = true)] public int? CategoryFilter { get; set; }
        [BindProperty(SupportsGet = true)] public string? SearchQuery { get; set; }
        public int TotalCount { get; private set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / Math.Max(1, PageSize));

        public void OnGet()
        {
            if (PageNumber <1) PageNumber =1;
            if (PageSize <1) PageSize =10;
            var skip = (PageNumber -1) * PageSize;

            // Use the new AdminSearch method from the service layer
            Items = _service.AdminSearch(
                SearchQuery, 
                VisibilityFilter ?? "all", 
                CategoryFilter, 
                skip, 
                PageSize, 
                out int totalCount
            );
            
            TotalCount = totalCount;

            // Preload categories dictionary for display
            var categories = _categoryService.GetAll().ToList();
            CategoryNames = categories.ToDictionary(c => c.Id, c => c.Name);

            // Build category filter dropdown options
            CategoryFilterOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "All Categories", Selected = !CategoryFilter.HasValue }
            };
            CategoryFilterOptions.AddRange(categories.OrderBy(c => c.Name).Select(c => 
                new SelectListItem 
                { 
                    Value = c.Id.ToString(), 
                    Text = c.Name,
                    Selected = CategoryFilter.HasValue && CategoryFilter.Value == c.Id
                }));
        }

        public IActionResult OnPostToggleVisibility(int id, int pageNumber, string visibilityFilter = "all", int? categoryFilter = null, string? searchQuery = null)
        {
            var link = _service.Get(id);
            if (link != null)
            {
                link.Visible = !link.Visible;
                _service.Update(link);
            }

            return RedirectToPage(new { pageNumber, visibilityFilter, categoryFilter, searchQuery });
        }
    }
}
