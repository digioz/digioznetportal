using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.Bo;

namespace digioz.Portal.Pages.Profile
{
    public class IndexModel : PageModel
    {
        private readonly IProfileService _profileService;
        private const int DefaultPageSize = 20;

        public IndexModel(IProfileService profileService)
        {
            _profileService = profileService;
        }

        public List<digioz.Portal.Bo.Profile> Profiles { get; private set; } = new();
        public int PageIndex { get; private set; }
        public int PageSize => DefaultPageSize;
        public int TotalRecords { get; private set; }
        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        public IActionResult OnGet(int page = 1, string? search = null)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                SearchTerm = search.Trim();
            }

            if (page < 1) page = 1;
            PageIndex = page;

            var all = _profileService.GetAll();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                var termLower = SearchTerm.ToLowerInvariant();
                all = all.Where(p => !string.IsNullOrEmpty(p.DisplayName) && p.DisplayName.ToLowerInvariant().Contains(termLower)).ToList();
            }

            var ordered = all
                .OrderBy(p => string.IsNullOrEmpty(p.DisplayName))
                .ThenBy(p => p.DisplayName)
                .ToList();

            TotalRecords = ordered.Count;
            Profiles = ordered.Skip((PageIndex - 1) * PageSize).Take(PageSize).ToList();

            return Page();
        }
    }
}
