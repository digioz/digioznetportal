using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Bo.ViewModels;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Pages.Links {
    public class IndexModel : PageModel {
        private readonly ILinkService _linkService;
        private readonly ILinkCategoryService _linkCategoryService;

        public IndexModel(ILinkService linkService, ILinkCategoryService linkCategoryService)
        {
            _linkService = linkService;
            _linkCategoryService = linkCategoryService;
        }

        [BindProperty(SupportsGet = true)]
        public int? id { get; set; }

        public Link? SelectedLink { get; set; }
        public List<LinkCategoryGroup> LinksByCategory { get; set; } = new();

        public void OnGet()
        {
            // If an ID is provided, show only that link
            if (id.HasValue)
            {
                SelectedLink = _linkService.Get(id.Value);
                if (SelectedLink != null && !SelectedLink.Visible)
                {
                    // Don't show non-visible links
                    SelectedLink = null;
                }
                return;
            }

            // Otherwise, show all links grouped by category
            var categories = _linkCategoryService.GetAll()
                .Where(c => c.Visible)
                .OrderBy(c => c.Name)
                .ToList();

            var links = _linkService.GetAll()
                .Where(l => l.Visible)
                .OrderBy(l => l.Name)
                .ToList();

            LinksByCategory = categories.Select(category => new LinkCategoryGroup
            {
                Category = category,
                Links = links.Where(l => l.LinkCategory == category.Id).ToList()
            }).Where(g => g.Links.Any()).ToList();
        }
    }
}