using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.ProductCategory
{
    public class DeleteModel : PageModel
    {
        private readonly IProductCategoryService _service;
        private readonly IProductService _productService;
        
        public DeleteModel(IProductCategoryService service, IProductService productService)
        { 
            _service = service;
            _productService = productService;
        }

        [BindProperty(SupportsGet = true)] public string? Id { get; set; }
        public Bo.ProductCategory? Item { get; private set; }

        public IActionResult OnGet(string id)
        {
            Item = _service.Get(id);
            if (Item == null) return RedirectToPage("/ProductCategory/Index", new { area = "Admin" });
            return Page();
        }

        public IActionResult OnPost()
        {
            if (string.IsNullOrEmpty(Id)) return RedirectToPage("/ProductCategory/Index", new { area = "Admin" });
            _productService.ClearProductCategory(Id);
            _service.Delete(Id);
            return RedirectToPage("/ProductCategory/Index", new { area = "Admin" });
        }
    }
}
