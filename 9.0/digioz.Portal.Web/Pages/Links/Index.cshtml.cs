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
                if (SelectedLink != null && (!SelectedLink.Visible || !(SelectedLink.Approved ?? false)))
                {
                    // Don't show non-visible or non-approved links
                    SelectedLink = null;
                }
                else if (SelectedLink != null)
                {
                    // Increment view count for the selected link
                    _linkService.IncrementViews(id.Value);
                }
                return;
            }

            // Otherwise, show all links grouped by category (only visible and approved)
            var categories = _linkCategoryService.GetAll()
                .Where(c => c.Visible)
                .OrderBy(c => c.Name)
                .ToList();

            var links = _linkService.GetAll()
                .Where(l => l.Visible && (l.Approved ?? false))
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