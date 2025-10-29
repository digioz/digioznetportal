using System;
using System.Linq;
using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.Page
{
    public class EditModel : PageModel
    {
        private readonly IPageService _service;
        private readonly IUserHelper _userHelper;
        public EditModel(IPageService service, IUserHelper userHelper)
        {
            _service = service;
            _userHelper = userHelper;
        }

        [BindProperty] public Bo.Page Item { get; set; }

        public IActionResult OnGet(int id)
        {
            Item = _service.Get(id);
            if (Item == null) return RedirectToPage("/Page/Index", new { area = "Admin" });
            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid) return Page();
            Item.Timestamp = DateTime.UtcNow;
            var email = User?.Identity?.Name;
            Item.UserId = _userHelper.GetUserIdByEmail(email);
            _service.Update(Item);
            return RedirectToPage("/Page/Index", new { area = "Admin" });
        }
    }
}
