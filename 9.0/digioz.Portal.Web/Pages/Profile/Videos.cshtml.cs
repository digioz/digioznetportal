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
    public class VideosModel : PageModel
    {
        private readonly IProfileService _profileService;
        private readonly IVideoService _videoService;
        private readonly IUserHelper _userHelper;

        public VideosModel(IProfileService profileService, IVideoService videoService, IUserHelper userHelper)
        {
            _profileService = profileService;
            _videoService = videoService;
            _userHelper = userHelper;
        }

        public digioz.Portal.Bo.Profile? UserProfile { get; private set; }
        public IReadOnlyList<Video> Videos { get; private set; } = Array.Empty<Video>();
        public string? DisplayName { get; private set; }
        public bool IsOwner { get; private set; }
        public bool IsAdmin { get; private set; }

        [BindProperty(SupportsGet = true)] public string? userId { get; set; } // carries DisplayName
        [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
        [BindProperty(SupportsGet = true)] public int PageSize { get; set; } = 12;
        public int TotalCount { get; private set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / Math.Max(1, PageSize));

        public IActionResult OnGet()
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                var email = User?.Identity?.IsAuthenticated == true ? User.Identity!.Name : null;
                if (string.IsNullOrEmpty(email)) return NotFound();
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

            var loginEmail = User?.Identity?.Name;
            var loggedInUserId = !string.IsNullOrEmpty(loginEmail) ? _userHelper.GetUserIdByEmail(loginEmail) : null;
            IsOwner = loggedInUserId != null && loggedInUserId == UserProfile.UserId;
            IsAdmin = User?.IsInRole("Admin") == true;

            var allVideos = _videoService.GetAll()
                .Where(v => v.UserId == UserProfile.UserId &&
                    (IsOwner || IsAdmin || v.Visible))
                .OrderByDescending(v => v.Timestamp)
                .ToList();

            TotalCount = allVideos.Count;
            if (PageNumber < 1) PageNumber = 1;
            if (PageSize < 1) PageSize = 12;
            var skip = (PageNumber - 1) * PageSize;
            Videos = allVideos.Skip(skip).Take(PageSize).ToList();
            return Page();
        }
    }
}
