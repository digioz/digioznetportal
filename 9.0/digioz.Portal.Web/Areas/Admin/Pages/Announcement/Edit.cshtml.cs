using System;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.Announcement
{
    public class EditModel : PageModel
    {
        private readonly IAnnouncementService _service;
        private readonly IUserHelper _userHelper;
        public EditModel(IAnnouncementService service, IUserHelper userHelper)
        {
            _service = service;
            _userHelper = userHelper;
        }

        [BindProperty] public Bo.Announcement? Item { get; set; }
        public IActionResult OnGet(int id)
        {
            Item = _service.Get(id);
            if (Item == null) return RedirectToPage("/Announcement/Index", new { area = "Admin" });
            return Page();
        }
        public IActionResult OnPost()
        {
            if (!ModelState.IsValid) return Page();
            if (Item == null) return RedirectToPage("/Announcement/Index", new { area = "Admin" });
            Item.Timestamp = DateTime.UtcNow;
            var email = User?.Identity?.Name;
            Item.UserId = !string.IsNullOrEmpty(email) ? _userHelper.GetUserIdByEmail(email) : null;
            _service.Update(Item);
            return RedirectToPage("/Announcement/Index", new { area = "Admin" });
        }
    }
}
