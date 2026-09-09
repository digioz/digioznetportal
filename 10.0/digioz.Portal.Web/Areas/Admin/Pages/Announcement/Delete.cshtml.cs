using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.Announcement
{
    public class DeleteModel : PageModel
    {
        private readonly IAnnouncementService _service;
        public DeleteModel(IAnnouncementService service) { _service = service; }

        [BindProperty(SupportsGet = true)] public int Id { get; set; }
        public Bo.Announcement? Item { get; private set; }

        public IActionResult OnGet(int id)
        {
            Item = _service.Get(id);
            if (Item == null) return RedirectToPage("/Announcement/Index", new { area = "Admin" });
            return Page();
        }

        public IActionResult OnPost()
        {
            _service.Delete(Id);
            return RedirectToPage("/Announcement/Index", new { area = "Admin" });
        }
    }
}
