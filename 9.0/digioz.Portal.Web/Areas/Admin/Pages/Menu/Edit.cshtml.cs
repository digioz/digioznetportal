using System;
using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;

namespace digioz.Portal.Web.Areas.Admin.Pages.Menu
{
    public class EditModel : PageModel
    {
        private readonly IMenuService _service;
        private readonly IUserHelper _userHelper;
        private readonly IMemoryCache _cache;
        public EditModel(IMenuService service, IUserHelper userHelper, IMemoryCache cache)
        {
            _service = service;
            _userHelper = userHelper;
            _cache = cache;
        }

        [BindProperty]
        public digioz.Portal.Bo.Menu? Item { get; set; }

        public IActionResult OnGet(int id)
        {
            var existing = _service.Get(id);
            if (existing == null) return NotFound();
            Item = existing;
            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid) return Page();
            if (Item == null) return NotFound();
            // Use no-tracking read to avoid multiple tracked instances in same DbContext
            var existing = _service.GetNoTracking(Item.Id);
            if (existing == null) return NotFound();

            // Keep existing SortOrder regardless of posted value
            var keepSort = existing.SortOrder;
            Item.Timestamp = DateTime.UtcNow;
            var email = User?.Identity?.Name;
            Item.UserId = !string.IsNullOrEmpty(email) ? _userHelper.GetUserIdByEmail(email) : null;
            Item.SortOrder = keepSort;

            _service.Update(Item);
            _cache.Remove("TopMenu");
            _cache.Remove("LeftMenu");
            return RedirectToPage("/Menu/Index", new { area = "Admin" });
        }
    }
}
