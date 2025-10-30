using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.Module
{
    public class DeleteModel : PageModel
    {
        private readonly IModuleService _service;
        public DeleteModel(IModuleService service) { _service = service; }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }
        public Bo.Module? Item { get; private set; }

        public IActionResult OnGet(int id)
        {
            Item = _service.Get(id);
            if (Item == null) return RedirectToPage("/Module/Index", new { area = "Admin" });
            return Page();
        }

        public IActionResult OnPost()
        {
            _service.Delete(Id);
            return RedirectToPage("/Module/Index", new { area = "Admin" });
        }
    }
}
