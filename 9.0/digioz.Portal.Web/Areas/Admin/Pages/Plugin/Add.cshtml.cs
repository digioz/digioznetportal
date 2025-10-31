using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.Plugin
{
    public class AddModel : PageModel
    {
        private readonly IPluginService _service;
        public AddModel(IPluginService service) { _service = service; }

        [BindProperty] public Bo.Plugin Item { get; set; } = new Bo.Plugin { IsEnabled = true };

        public void OnGet() { }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid) return Page();
            _service.Add(Item);
            return RedirectToPage("/Plugin/Index", new { area = "Admin" });
        }
    }
}
