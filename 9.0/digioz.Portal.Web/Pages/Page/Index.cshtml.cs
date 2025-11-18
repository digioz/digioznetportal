using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Pages.Page
{
    public class IndexModel : PageModel
    {
        private readonly IPageService _pageService;
        public IndexModel(IPageService pageService)
        {
            _pageService = pageService;
        }

        [BindProperty(SupportsGet = true)]
        public string? Id { get; set; }

        public string BodyHtml { get; private set; } = string.Empty;

        public IActionResult OnGet(string? id)
        {
            var key = id ?? Id;

            if (!string.IsNullOrWhiteSpace(key) && int.TryParse(key, out var numericId))
            {
                var page = _pageService.Get(numericId);
                if (page == null) return NotFound();
                BodyHtml = page.Body ?? string.Empty;
                return Page();
            }
            else if (!string.IsNullOrWhiteSpace(key))
            {
                var page = _pageService.GetByUrl(key);
                if (page == null) return NotFound();
                BodyHtml = page.Body ?? string.Empty;
                return Page();
            }

            return NotFound();
        }
    }
}