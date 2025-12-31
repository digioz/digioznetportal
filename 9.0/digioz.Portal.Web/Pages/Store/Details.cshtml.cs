using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Pages.Store {
    public class DetailsModel : PageModel {
        private readonly IProductService _productService;
        private readonly IProductCategoryService _categoryService;
        private readonly IProductOptionService _optionService;

        public DetailsModel(
            IProductService productService,
            IProductCategoryService categoryService,
            IProductOptionService optionService) {
            _productService = productService;
            _categoryService = categoryService;
            _optionService = optionService;
        }

        public Product? Product { get; set; }
        public string? CategoryName { get; set; }
        public List<ProductOption> ProductOptions { get; set; } = new();

        public void OnGet(string id) {
            Product = _productService.Get(id);

            if (Product == null)
                return;

            // Get category
            if (!string.IsNullOrEmpty(Product.ProductCategoryId)) {
                var category = _categoryService.Get(Product.ProductCategoryId);
                CategoryName = category?.Name ?? null;
            }

            // Get product options
            ProductOptions = _optionService.GetAll()
                .Where(o => o.ProductId == id)
                .ToList();

            // Increment view count
            _productService.IncrementViews(id);
        }
    }
}