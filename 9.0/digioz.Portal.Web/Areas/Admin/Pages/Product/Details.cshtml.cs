using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.Product
{
    public class DetailsModel : PageModel
    {
        private readonly IProductService _service;
        private readonly IProductCategoryService _categoryService;
        private readonly IProductOptionService _optionService;

        public DetailsModel(
            IProductService service,
            IProductCategoryService categoryService,
            IProductOptionService optionService)
        {
            _service = service;
            _categoryService = categoryService;
            _optionService = optionService;
        }

        public Bo.Product? Item { get; private set; }
        public string? CategoryName { get; private set; }
        public List<Bo.ProductOption> ProductOptions { get; private set; } = new();

        public IActionResult OnGet(string id)
        {
            Item = _service.Get(id);
            if (Item == null) return RedirectToPage("/Product/Index", new { area = "Admin" });

            // Get category name
            if (!string.IsNullOrEmpty(Item.ProductCategoryId))
            {
                var category = _categoryService.Get(Item.ProductCategoryId);
                CategoryName = category?.Name;
            }

            // Get product options
            ProductOptions = _optionService.GetAll().Where(o => o.ProductId == id).ToList();

            return Page();
        }
    }
}
