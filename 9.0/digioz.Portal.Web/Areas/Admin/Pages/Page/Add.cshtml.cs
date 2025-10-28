using System;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.Page
{
    public class AddModel : PageModel
    {
        private readonly IPageService _service;
        private readonly IUserHelper _userHelper;
        public AddModel(IPageService service, IUserHelper userHelper)
        {
            _service = service;
            _userHelper = userHelper;
        }

        [BindProperty] public Bo.Page Item { get; set; } = new Bo.Page { Visible = true, Timestamp = DateTime.UtcNow };

        public void OnGet() { }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid) return Page();
            Item.Timestamp = DateTime.UtcNow;
            var email = User?.Identity?.Name;
            Item.UserId = _userHelper.GetUserIdByEmail(email);
            _service.Add(Item);
            return RedirectToPage("/Page/Index", new { area = "Admin" });
        }
    }
}
