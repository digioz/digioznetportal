using System;
using System.Linq;
using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;

namespace digioz.Portal.Web.Areas.Admin.Pages.Menu
{
    public class AddModel : PageModel
    {
        private readonly IMenuService _service;
        private readonly IUserHelper _userHelper;
        private readonly IMemoryCache _cache;
        public AddModel(IMenuService service, IUserHelper userHelper, IMemoryCache cache)
        {
            _service = service;
            _userHelper = userHelper;
            _cache = cache;
        }

        [BindProperty]
        public digioz.Portal.Bo.Menu Item { get; set; } = new digioz.Portal.Bo.Menu { Visible = true, Timestamp = DateTime.UtcNow };

        public void OnGet() { }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid) return Page();

            // Assign audit fields
            Item.Timestamp = DateTime.UtcNow;
            var email = User?.Identity?.Name;
            Item.UserId = !string.IsNullOrEmpty(email) ? _userHelper.GetUserIdByEmail(email) : null;

            // SortOrder: set to next available integer globally
            var all = _service.GetAll();
            var max = all.Count == 0 ? 0 : all.Max(m => m.SortOrder);
            Item.SortOrder = max + 1;

            _service.Add(Item);
            _cache.Remove("TopMenu");
            _cache.Remove("LeftMenu");
            return RedirectToPage("/Menu/Index", new { area = "Admin" });
        }
    }
}
