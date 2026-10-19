using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.Utilities;

namespace digioz.Portal.Pages.Profile
{
    public class PicturesModel : PageModel
    {
        private readonly IProfileService _profileService;
        private readonly IPictureService _pictureService;
        private readonly IUserHelper _userHelper;

        public PicturesModel(IProfileService profileService, IPictureService pictureService, IUserHelper userHelper)
        {
            _profileService = profileService;
            _pictureService = pictureService;
            _userHelper = userHelper;
        }

        public digioz.Portal.Bo.Profile? UserProfile { get; private set; }
        public IReadOnlyList<Picture> Pictures { get; private set; } = Array.Empty<Picture>();
        public string? DisplayName { get; private set; }
        public bool IsOwner { get; private set; }
        public bool IsAdmin { get; private set; }

        [BindProperty(SupportsGet = true)] public string? userId { get; set; }
        [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
        [BindProperty(SupportsGet = true)] public int PageSize { get; set; } = 12;
        public int TotalCount { get; private set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / Math.Max(1, PageSize));

        public IActionResult OnGet()
        {
            // If DisplayName (userId param) not provided, attempt to resolve from logged in user
            if (string.IsNullOrWhiteSpace(userId))
            {
                var email = User?.Identity?.IsAuthenticated == true ? User.Identity!.Name : null;
                if (string.IsNullOrEmpty(email)) return NotFound(); // not logged in and no display name supplied
                var currentUserId = _userHelper.GetUserIdByEmail(email);
                if (currentUserId == null) return NotFound();
                UserProfile = _profileService.GetByUserId(currentUserId);
                if (UserProfile == null) return NotFound();
                DisplayName = UserProfile.DisplayName;
            }
            else
            {
                DisplayName = userId.Trim();
                UserProfile = _profileService.GetAll().FirstOrDefault(p => p.DisplayName != null && p.DisplayName.Equals(DisplayName, StringComparison.OrdinalIgnoreCase));
                if (UserProfile == null) return NotFound();
            }

            // Determine ownership/admin
            var loginEmail = User?.Identity?.Name;
            var loggedInUserId = !string.IsNullOrEmpty(loginEmail) ? _userHelper.GetUserIdByEmail(loginEmail) : null;
            IsOwner = loggedInUserId != null && loggedInUserId == UserProfile.UserId;
            IsAdmin = User?.IsInRole("Admin") == true;

            // Use LINQ filtering consistent with codebase pattern
            var allPictures = _pictureService.GetAll()
                .Where(p => p.UserId == UserProfile.UserId && ((IsOwner || IsAdmin) || p.Approved))
                .OrderByDescending(p => p.Timestamp)
                .ToList();

            TotalCount = allPictures.Count;
            if (PageNumber < 1) PageNumber = 1;
            if (PageSize < 1) PageSize = 12;
            var skip = (PageNumber - 1) * PageSize;
            Pictures = allPictures.Skip(skip).Take(PageSize).ToList();
            return Page();
        }
    }
}
