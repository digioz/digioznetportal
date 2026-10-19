using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.Link
{
    public class DeleteModel : PageModel
    {
        private readonly ILinkService _service;
        public DeleteModel(ILinkService service) { _service = service; }

        [BindProperty(SupportsGet = true)] public int Id { get; set; }
        public digioz.Portal.Bo.Link? Item { get; private set; }

        public IActionResult OnGet(int id)
        {
            Item = _service.Get(id);
            if (Item == null) return RedirectToPage("/Link/Index", new { area = "Admin" });
            return Page();
        }

        public IActionResult OnPost()
        {
            _service.Delete(Id);
            return RedirectToPage("/Link/Index", new { area = "Admin" });
        }
    }
}
