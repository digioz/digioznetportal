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
        
        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;
        
        public int PageSize => DefaultPageSize;
        public int TotalRecords { get; private set; }
        
        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        public IActionResult OnGet(string? search = null)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                SearchTerm = search.Trim();
            }

            if (PageNumber < 1) PageNumber = 1;

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
            Profiles = ordered.Skip((PageNumber - 1) * PageSize).Take(PageSize).ToList();

            return Page();
        }
    }
}
