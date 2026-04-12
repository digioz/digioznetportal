using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.Config
{
    public class DeleteModel : PageModel
    {
        private readonly IConfigService _service;
        public DeleteModel(IConfigService service) { _service = service; }

        [BindProperty(SupportsGet = true)] public string Id { get; set; } = string.Empty;
        public digioz.Portal.Bo.Config? Item { get; private set; }

        public IActionResult OnGet(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            Item = _service.Get(id);
            if (Item == null) return RedirectToPage("/Config/Index", new { area = "Admin" });
            return Page();
        }

        public IActionResult OnPost()
        {
            if (string.IsNullOrEmpty(Id)) return RedirectToPage("/Config/Index", new { area = "Admin" });
            _service.Delete(Id);
            return RedirectToPage("/Config/Index", new { area = "Admin" });
        }
    }
}
