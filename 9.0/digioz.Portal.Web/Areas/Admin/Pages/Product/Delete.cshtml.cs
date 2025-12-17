using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.Product
{
    public class DeleteModel : PageModel
    {
        private readonly IProductService _service;
        private readonly IProductOptionService _optionService;
        
        public DeleteModel(IProductService service, IProductOptionService optionService)
        {
            _service = service;
            _optionService = optionService;
        }

        [BindProperty(SupportsGet = true)] public string? Id { get; set; }
        public Bo.Product? Item { get; private set; }

        public IActionResult OnGet(string id)
        {
            Item = _service.Get(id);
            if (Item == null) return RedirectToPage("/Product/Index", new { area = "Admin" });
            return Page();
        }

        public IActionResult OnPost()
        {
            if (string.IsNullOrEmpty(Id)) return RedirectToPage("/Product/Index", new { area = "Admin" });
            
            // Delete all options for this product first
            var options = _optionService.GetAll().FindAll(o => o.ProductId == Id);
            foreach (var option in options)
            {
                _optionService.Delete(option.Id);
            }
            
            _service.Delete(Id);
            return RedirectToPage("/Product/Index", new { area = "Admin" });
        }
    }
}
