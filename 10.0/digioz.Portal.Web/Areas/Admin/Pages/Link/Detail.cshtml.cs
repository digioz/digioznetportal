using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.Link
{
    public class DetailModel : PageModel
    {
        private readonly ILinkService _service;
        private readonly ILinkCategoryService _categoryService;
        public DetailModel(ILinkService service, ILinkCategoryService categoryService)
        { _service = service; _categoryService = categoryService; }

        public digioz.Portal.Bo.Link? Item { get; private set; }
        public string CategoryName { get; private set; } = "";

        public IActionResult OnGet(int id)
        {
            Item = _service.Get(id);
            if (Item == null) return RedirectToPage("/Link/Index", new { area = "Admin" });
            var category = _categoryService.Get(Item.LinkCategory);
            CategoryName = category?.Name ?? "-";
            return Page();
        }
    }
}
