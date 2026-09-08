using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.Announcement
{
    public class DetailsModel : PageModel
    {
        private readonly IAnnouncementService _service;
        public DetailsModel(IAnnouncementService service) { _service = service; }
        public Bo.Announcement? Item { get; private set; }
        public IActionResult OnGet(int id)
        {
            Item = _service.Get(id);
            if (Item == null) return RedirectToPage("/Announcement/Index", new { area = "Admin" });
            return Page();
        }
    }
}
