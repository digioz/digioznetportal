using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.RssManager
{
    public class DeleteModel : PageModel
    {
        private readonly IRssService _service;
        public DeleteModel(IRssService service) { _service = service; }

        [BindProperty(SupportsGet = true)] public int Id { get; set; }
        public Rss? Item { get; private set; }

        public IActionResult OnGet()
        {
            Item = _service.Get(Id);
            if (Item == null) return NotFound();
            return Page();
        }

        public IActionResult OnPost()
        {
            var item = _service.Get(Id);
            if (item == null) return NotFound();
            _service.Delete(Id);
            return RedirectToPage("/RssManager/Index", new { area = "Admin" });
        }
    }
}
