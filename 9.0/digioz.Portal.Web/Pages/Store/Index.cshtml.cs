using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Pages.Store
{
    public class IndexModel : PageModel
    {
        private readonly IProductService _productService;
        private const int PAGE_SIZE = 12;

        public IndexModel(IProductService productService)
        {
            _productService = productService;
        }

        public List<Product> Products { get; set; } = new();
        public int pageNumber { get; set; } = 1;
        public int pageSize => PAGE_SIZE;
        public int TotalCount { get; set; }
        public int TotalPages => (int)System.Math.Ceiling((double)TotalCount / PAGE_SIZE);

        public void OnGet(int pageNumber = 1)
        {
            this.pageNumber = pageNumber < 1 ? 1 : pageNumber;

            // Get all visible products ordered by most recent
            var allProducts = _productService.GetAll()
                .Where(p => p.Visible)
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