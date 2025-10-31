using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.CommentConfig
{
    public class DeleteModel : PageModel
    {
        private readonly ICommentConfigService _service;
        public DeleteModel(ICommentConfigService service) { _service = service; }

        [BindProperty(SupportsGet = true)] public string Id { get; set; }
        public Bo.CommentConfig? Item { get; private set; }

        public IActionResult OnGet(string id)
        {
            Item = _service.Get(id);
            if (Item == null) return RedirectToPage("/CommentConfig/Index", new { area = "Admin" });
            return Page();
        }

        public IActionResult OnPost()
        {
            _service.Delete(Id);
            return RedirectToPage("/CommentConfig/Index", new { area = "Admin" });
        }
    }
}
