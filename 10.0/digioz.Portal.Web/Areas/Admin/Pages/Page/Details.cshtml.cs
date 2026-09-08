using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.Page
{
    public class DetailsModel : PageModel
    {
        private readonly IPageService _service;
        public DetailsModel(IPageService service) { _service = service; }

        public Bo.Page? Item { get; private set; }

        public IActionResult OnGet(int id)
        {
            Item = _service.Get(id);
            if (Item == null) return RedirectToPage("/Page/Index", new { area = "Admin" });
            return Page();
        }
    }
}
