using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.Module
{
    public class EditModel : PageModel
    {
        private readonly IModuleService _service;
        public EditModel(IModuleService service) { _service = service; }

        [BindProperty]
        public Bo.Module Item { get; set; }

        public IActionResult OnGet(int id)
        {
            Item = _service.Get(id);
            if (Item == null) return RedirectToPage("/Module/Index", new { area = "Admin" });
            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid) return Page();
            _service.Update(Item);
            return RedirectToPage("/Module/Index", new { area = "Admin" });
        }
    }
}
