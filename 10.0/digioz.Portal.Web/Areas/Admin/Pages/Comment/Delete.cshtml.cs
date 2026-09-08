using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.Comment
{
    public class DeleteModel : PageModel
    {
        private readonly ICommentService _service;
        public DeleteModel(ICommentService service) { _service = service; }

        [BindProperty(SupportsGet = true)]
        public string Id { get; set; } = string.Empty;
        public Bo.Comment? Item { get; private set; }

        public IActionResult OnGet(string id)
        {
            Item = _service.Get(id);
            if (Item == null) return RedirectToPage("/Comment/Index", new { area = "Admin" });
            return Page();
        }

        public IActionResult OnPost()
        {
            if (!string.IsNullOrEmpty(Id))
            {
                _service.Delete(Id);
            }
            return RedirectToPage("/Comment/Index", new { area = "Admin" });
        }
    }
}
