using System;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.Announcement
{
    public class AddModel : PageModel
    {
        private readonly IAnnouncementService _service;
        private readonly IUserHelper _userHelper;
        public AddModel(IAnnouncementService service, IUserHelper userHelper)
        {
            _service = service;
            _userHelper = userHelper;
        }

        [BindProperty] public Bo.Announcement Item { get; set; } = new Bo.Announcement { Visible = true, Timestamp = DateTime.UtcNow };

        public void OnGet() { }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid) return Page();
            Item.Timestamp = DateTime.UtcNow;
            var email = User?.Identity?.Name;
            Item.UserId = _userHelper.GetUserIdByEmail(email);
            _service.Add(Item);
            return RedirectToPage("/Announcement/Index", new { area = "Admin" });
        }
    }
}
