using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;

namespace digioz.Portal.Pages.Profile
{
    public class DetailsModel : PageModel
    {
        private readonly IProfileService _profileService;
        private readonly ICommentService _commentService;
        private readonly IPictureService _pictureService;
        private readonly IVideoService _videoService;
        private readonly IThemeService _themeService;

        public DetailsModel(IProfileService profileService, ICommentService commentService, IPictureService pictureService, IVideoService videoService, IThemeService themeService)
        {
            _profileService = profileService;
            _commentService = commentService;
            _pictureService = pictureService;
            _videoService = videoService;
            _themeService = themeService;
        }

        public digioz.Portal.Bo.Profile? UserProfile { get; private set; }
        public List<Comment> RecentComments { get; private set; } = new();
        public List<Picture> RecentPictures { get; private set; } = new();
        public List<Video> RecentVideos { get; private set; } = new();
        public string? DisplayName { get; private set; }
        public string? ThemeName { get; private set; }
        public string? CurrentUserDisplayName { get; private set; }

        public IActionResult OnGet(string? userId)
        {
            // userId query string carries DisplayName per requirements
            if (string.IsNullOrWhiteSpace(userId)) return NotFound();
            DisplayName = userId.Trim();

            // Get current user's display name for comparison
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(currentUserId))
            {
                var currentProfile = _profileService.GetAll().FirstOrDefault(p => p.UserId == currentUserId);
                CurrentUserDisplayName = currentProfile?.DisplayName;
            }

            UserProfile = _profileService.GetAll()
                .FirstOrDefault(p => p.DisplayName != null && p.DisplayName.Equals(DisplayName, StringComparison.OrdinalIgnoreCase));
            if (UserProfile == null) return NotFound();

            // Get theme name if user has a theme selected
            if (UserProfile.ThemeId.HasValue)
            {
                var theme = _themeService.Get(UserProfile.ThemeId.Value);
                ThemeName = theme?.Name;
            }
            else
            {
                var defaultTheme = _themeService.GetDefault();
                ThemeName = defaultTheme != null ? $"{defaultTheme.Name} (Default)" : "Default";
            }

            // Comments authored by this user (latest 5)
            RecentComments = _commentService.GetAll()
                .Where(c => c.UserId == UserProfile.UserId)
                .OrderByDescending(c => c.ModifiedDate ?? c.CreatedDate)
                .Take(5)
                .ToList();

            // Pictures (approved & visible) latest 6
            RecentPictures = _pictureService.GetAll()
                .Where(p => p.UserId == UserProfile.UserId && p.Visible && p.Approved)
                .OrderByDescending(p => p.Timestamp)
                .Take(6)
                .ToList();

            // Videos (approved & visible) latest 6
            RecentVideos = _videoService.GetAll()
                .Where(v => v.UserId == UserProfile.UserId && v.Visible && v.Approved)
                .OrderByDescending(v => v.Timestamp)
                .Take(6)
                .ToList();

            return Page();
        }
    }
}