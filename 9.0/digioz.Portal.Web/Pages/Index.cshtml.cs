using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Linq;

namespace digioz.Portal.Pages
{
    public class IndexModel : PageModel
    {
        [BindProperty]
        public digioz.Portal.Bo.Page? PageContent { get; set; }
        [BindProperty]
        public bool AllowComments { get; set; }

        private readonly IPageService _pageService;
        private readonly ICommentsHelper _commentsHelper;
        private readonly IConfigService _configService;
        private readonly ICommentConfigService _commentConfigService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(
            ILogger<IndexModel> logger,
            IPageService pageService,
            ICommentsHelper commentsHelper,
            IConfigService configService,
            ICommentConfigService commentConfigService)
        {
            _pageService = pageService;
            _commentsHelper = commentsHelper;
            _configService = configService;
            _commentConfigService = commentConfigService;
            _logger = logger;
        }

        public void OnGet()
        {
            PageContent = _pageService.GetByTitle("Home");
            _logger.LogInformation("Loaded Home page content. Title={Title}", PageContent?.Title);


            // Display comments if enabled for this page
            AllowComments = _commentsHelper.IsCommentsEnabledForPageTitle(PageContent?.Title);
        }
    }
}
