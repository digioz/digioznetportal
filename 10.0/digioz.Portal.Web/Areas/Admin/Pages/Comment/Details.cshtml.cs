using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.Comment
{
    public class DetailsModel : PageModel
    {
        private readonly ICommentService _service;
        public DetailsModel(ICommentService service) { _service = service; }

        public Bo.Comment? Item { get; set; }

        public IActionResult OnGet(string id)
        {
            if (string.IsNullOrEmpty(id)) return RedirectToPage("/Comment/Index", new { area = "Admin" });
            Item = _service.Get(id);
            if (Item == null) return RedirectToPage("/Comment/Index", new { area = "Admin" });
            return Page();
        }
    }
}
