using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;

namespace digioz.Portal.Web.Areas.Admin.Pages.CommentConfig
{
    public class DeleteModel : PageModel
    {
        private readonly ICommentConfigService _service;
        private readonly IMemoryCache _cache;
        public DeleteModel(ICommentConfigService service, IMemoryCache cache) { _service = service; _cache = cache; }

        [BindProperty(SupportsGet = true)] public string? Id { get; set; }
        public Bo.CommentConfig? Item { get; private set; }

        public IActionResult OnGet(string id)
        {
            Item = _service.Get(id);
            if (Item == null) return RedirectToPage("/CommentConfig/Index", new { area = "Admin" });
            return Page();
        }

        public IActionResult OnPost()
        {
            if (string.IsNullOrEmpty(Id)) return RedirectToPage("/CommentConfig/Index", new { area = "Admin" });
            _service.Delete(Id);
            CacheKeys.InvalidateCommentConfig(_cache);
            return RedirectToPage("/CommentConfig/Index", new { area = "Admin" });
        }
    }
}
