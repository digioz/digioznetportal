using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Pages.Store
{
    public class ByCategoryModel : PageModel
    {
        private readonly IProductService _productService;
        private readonly IProductCategoryService _categoryService;
        private const int PAGE_SIZE = 12;

        public ByCategoryModel(IProductService productService, IProductCategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }

        public List<Product> Products { get; set; } = new();
        public string? CategoryName { get; set; }
        public string? CategoryDescription { get; set; }
        public int pageNumber { get; set; } = 1;
        public int pageSize => PAGE_SIZE;
        public int TotalCount { get; set; }
        public int TotalPages => (int)System.Math.Ceiling((double)TotalCount / PAGE_SIZE);

        public void OnGet(string id, int pageNumber = 1)
        {
            this.pageNumber = pageNumber < 1 ? 1 : pageNumber;

            // Get category
            var category = _categoryService.Get(id);
            if (category == null)
            {
                CategoryName = "Category Not Found";
                return;
            }

            CategoryName = category.Name;
            CategoryDescription = category.Description;

            // Get all visible products in this category
            var allProducts = _productService.GetAll()
                .Where(p => p.Visible && p.ProductCategoryId == id)
                .OrderByDescending(p => p.DateCreated)
                .ToList();

            TotalCount = allProducts.Count;

            // Apply pagination
            Products = allProducts
                .Skip((this.pageNumber - 1) * PAGE_SIZE)
                .Take(PAGE_SIZE)
                .ToList();
        }
    }
}