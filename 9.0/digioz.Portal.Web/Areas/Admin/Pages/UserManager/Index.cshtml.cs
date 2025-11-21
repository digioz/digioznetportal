using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.Bo;

namespace digioz.Portal.Web.Areas.Admin.Pages.UserManager
{
    [Authorize(Roles = "Administrator")]
    public class IndexModel : PageModel
    {
        private readonly IAspNetUserService _userService;
        private readonly IProfileService _profileService;
        private readonly UserManager<IdentityUser> _userManager;

        public IndexModel(IAspNetUserService userService, IProfileService profileService, UserManager<IdentityUser> userManager)
        {
            _userService = userService;
            _profileService = profileService;
            _userManager = userManager;
        }

        public List<UserWithProfile> Users { get; set; } = new();
        
        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; } = 20;
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }

        public async Task OnGetAsync()
        {
            // Get all users and profiles (these are typically small datasets for admin pages)
            var allUsers = _userService.GetAll();
            var allProfiles = _profileService.GetAll();

            // Combine users with their profiles and check if they're protected
            var usersWithProfiles = new List<UserWithProfile>();
            foreach (var user in allUsers)
            {
                var profile = allProfiles.FirstOrDefault(p => p.UserId == user.Id);
                var identityUser = await _userManager.FindByIdAsync(user.Id);
                var isProtected = false;

                if (identityUser != null)
                {
                    // Check if user has Administrator role
                    var roles = await _userManager.GetRolesAsync(identityUser);
                    var hasAdminRole = roles.Contains("Administrator", StringComparer.OrdinalIgnoreCase);
                    
                    // Check if user is "System" user by display name
                    var isSystemUser = profile?.DisplayName != null && 
                                      profile.DisplayName.Equals("System", StringComparison.OrdinalIgnoreCase);
                    
                    isProtected = hasAdminRole || isSystemUser;
                }

                usersWithProfiles.Add(new UserWithProfile
                {
                    User = user,
                    Profile = profile,
                    IsProtected = isProtected
                });
            }

            // Apply search filter (in-memory filtering is acceptable here since user counts are typically small)
            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                var searchLower = SearchTerm.ToLowerInvariant();
                usersWithProfiles = usersWithProfiles.Where(up =>
                    (up.User.UserName != null && up.User.UserName.ToLowerInvariant().Contains(searchLower)) ||
                    (up.User.Email != null && up.User.Email.ToLowerInvariant().Contains(searchLower)) ||
                    (up.Profile?.DisplayName != null && up.Profile.DisplayName.ToLowerInvariant().Contains(searchLower)) ||
                    (up.Profile?.FirstName != null && up.Profile.FirstName.ToLowerInvariant().Contains(searchLower)) ||
                    (up.Profile?.LastName != null && up.Profile.LastName.ToLowerInvariant().Contains(searchLower))
                ).ToList();
            }

            TotalCount = usersWithProfiles.Count;
            TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);

            // Apply pagination
            if (PageNumber < 1) PageNumber = 1;
            if (PageNumber > TotalPages && TotalPages > 0) PageNumber = TotalPages;

            Users = usersWithProfiles
                .OrderBy(up => up.User.UserName)
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }

        public class UserWithProfile
        {
            public required AspNetUser User { get; set; }
            public Profile? Profile { get; set; }
            public bool IsProtected { get; set; }
        }
    }
}
