using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Pages
{
    public class IndexModel : PageModel
    {
        [BindProperty]
        public digioz.Portal.Bo.Page PageContent { get; set; }

        private readonly IPageService _pageService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger, IPageService pageService)
        {
            _pageService = pageService;
            _logger = logger;
        }

        public void OnGet()
        {
            PageContent = _pageService.GetByTitle("Home");
        }
    }
}
