using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.Plugin
{
    public class EditModel : PageModel
    {
        private readonly IPluginService _service;
        public EditModel(IPluginService service) { _service = service; }

        [BindProperty] public Bo.Plugin Item { get; set; }

        public IActionResult OnGet(int id)
        {
            Item = _service.Get(id);
            if (Item == null) return RedirectToPage("/Plugin/Index", new { area = "Admin" });
            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid) return Page();
            _service.Update(Item);
            return RedirectToPage("/Plugin/Index", new { area = "Admin" });
        }
    }
}
