using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Pages.Page
{
    public class IndexModel : PageModel
    {
        private readonly IPageService _pageService;
        private readonly ICommentsHelper _commentsHelper;

        public IndexModel(IPageService pageService, ICommentsHelper commentsHelper)
        {
            _pageService = pageService;
            _commentsHelper = commentsHelper;
        }

        [BindProperty(SupportsGet = true)]
        public string? Id { get; set; }

        public string BodyHtml { get; private set; } = string.Empty;
        public digioz.Portal.Bo.Page? PageContent { get; private set; }
        public bool AllowComments { get; private set; }

        public IActionResult OnGet(string? id)
        {
            var key = id ?? Id;

            if (!string.IsNullOrWhiteSpace(key) && int.TryParse(key, out var numericId))
            {
                PageContent = _pageService.Get(numericId);
                if (PageContent == null) return NotFound();
                BodyHtml = PageContent.Body ?? string.Empty;
                AllowComments = _commentsHelper.IsCommentsEnabledForPageTitle(PageContent.Title);
                return Page();
            }
            else if (!string.IsNullOrWhiteSpace(key))
            {
                PageContent = _pageService.GetByUrl(key);
                if (PageContent == null) return NotFound();
                BodyHtml = PageContent.Body ?? string.Empty;
                AllowComments = _commentsHelper.IsCommentsEnabledForPageTitle(PageContent.Title);
                return Page();
            }

            return NotFound();
        }
    }
}