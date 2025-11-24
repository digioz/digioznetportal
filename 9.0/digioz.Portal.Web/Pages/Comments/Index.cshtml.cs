using System;
using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProfileEntity = digioz.Portal.Bo.Profile;

namespace digioz.Portal.Web.Pages.Comments
{
    public class IndexModel : PageModel
    {
        private readonly ICommentService _commentService;
        private readonly IProfileService _profileService;
        private readonly IPictureService _pictureService;
        private readonly IVideoService _videoService;

        public IndexModel(
            ICommentService commentService,
            IProfileService profileService,
            IPictureService pictureService,
            IVideoService videoService)
        {
            _commentService = commentService;
            _profileService = profileService;
            _pictureService = pictureService;
            _videoService = videoService;
        }

        public List<CommentViewModel> Comments { get; set; } = new();
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        public void OnGet(int pageNumber = 1)
        {
            // Validate page number
            PageNumber = pageNumber > 0 ? pageNumber : 1;

            // Get all comments ordered by newest first
            var allComments = _commentService.GetAll()
                .OrderByDescending(c => c.CreatedDate)
                .ToList();

            TotalCount = allComments.Count;

            // Apply pagination
            var pagedComments = allComments
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            // Batch load all related data to avoid N+1 queries
            var userIds = pagedComments
                .Where(c => !string.IsNullOrEmpty(c.UserId))
                .Select(c => c.UserId)
                .Distinct()
                .ToList();

            var pictureIds = pagedComments
                .Where(c => c.ReferenceType == "/Pictures/Details" && !string.IsNullOrWhiteSpace(c.ReferenceId))
                .Select(c => c.ReferenceId)
                .Where(id => int.TryParse(id, out _))
                .Select(id => int.Parse(id))
                .Distinct()
                .ToList();

            var videoIds = pagedComments
                .Where(c => c.ReferenceType == "/Videos/Details" && !string.IsNullOrWhiteSpace(c.ReferenceId))
                .Select(c => c.ReferenceId)
                .Where(id => int.TryParse(id, out _))
                .Select(id => int.Parse(id))
                .Distinct()
                .ToList();

            // Fetch all related data in batch queries
            var profiles = _profileService.GetByUserIds(userIds).ToDictionary(p => p.UserId, p => p);
            var pictures = _pictureService.GetByIds(pictureIds).ToDictionary(p => p.Id, p => p);
            var videos = _videoService.GetByIds(videoIds).ToDictionary(v => v.Id, v => v);

            // Build view models using pre-loaded data
            Comments = pagedComments.Select(c => CreateCommentViewModel(c, profiles, pictures, videos)).ToList();
        }

        private CommentViewModel CreateCommentViewModel(
            Comment comment,
            Dictionary<string, ProfileEntity> profiles,
            Dictionary<int, Picture> pictures,
            Dictionary<int, Video> videos)
        {
            var viewModel = new CommentViewModel
            {
                Comment = comment,
                Profile = null,
                ThumbnailUrl = null,
                DetailsUrl = null,
                DisplayName = comment.Username ?? "Anonymous User",
                AvatarUrl = "/img/avatar/thumb/default.png"
            };

            // Get user profile information from pre-loaded dictionary
            if (!string.IsNullOrEmpty(comment.UserId) && profiles.TryGetValue(comment.UserId, out var profile))
            {
                viewModel.Profile = profile;
                // Use DisplayName if available, otherwise construct from first/last name
                if (!string.IsNullOrWhiteSpace(profile.DisplayName))
                {
                    viewModel.DisplayName = profile.DisplayName.Trim();
                }
                else if (!string.IsNullOrWhiteSpace(profile.FirstName) || !string.IsNullOrWhiteSpace(profile.LastName))
                {
                    viewModel.DisplayName = $"{profile.FirstName} {profile.LastName}".Trim();
                }

                // Limit display name length
                if (viewModel.DisplayName.Length > 100)
                {
                    viewModel.DisplayName = viewModel.DisplayName.Substring(0, 100);
                }

                // Get avatar - sanitize filename to prevent path traversal
                if (!string.IsNullOrWhiteSpace(profile.Avatar))
                {
                    var avatarFilename = System.IO.Path.GetFileName(profile.Avatar.Trim());
                    // Validate file extension
                    if (!string.IsNullOrWhiteSpace(avatarFilename) && 
                        System.Text.RegularExpressions.Regex.IsMatch(avatarFilename, @"^[a-zA-Z0-9_\-\.]+\.(jpg|jpeg|png|gif|webp)$", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                    {
                        viewModel.AvatarUrl = $"/img/avatar/thumb/{avatarFilename}";
                    }
                }
            }

            // Determine media type and get thumbnail/details URL from pre-loaded dictionaries
            if (comment.ReferenceType == "/Pictures/Details" && !string.IsNullOrWhiteSpace(comment.ReferenceId))
            {
                if (int.TryParse(comment.ReferenceId, out var pictureId) && pictures.TryGetValue(pictureId, out var picture))
                {
                    if (!string.IsNullOrWhiteSpace(picture.Thumbnail))
                    {
                        // Sanitize thumbnail filename
                        var thumbnailFilename = System.IO.Path.GetFileName(picture.Thumbnail);
                        if (!string.IsNullOrWhiteSpace(thumbnailFilename))
                        {
                            viewModel.ThumbnailUrl = $"/img/Pictures/Thumb/{thumbnailFilename}";
                            viewModel.DetailsUrl = $"/Pictures/Details/{pictureId}";
                        }
                    }
                }
            }
            else if (comment.ReferenceType == "/Videos/Details" && !string.IsNullOrWhiteSpace(comment.ReferenceId))
            {
                if (int.TryParse(comment.ReferenceId, out var videoId) && videos.TryGetValue(videoId, out var video))
                {
                    if (!string.IsNullOrWhiteSpace(video.Thumbnail))
                    {
                        // Sanitize thumbnail filename
                        var thumbnailFilename = System.IO.Path.GetFileName(video.Thumbnail);
                        if (!string.IsNullOrWhiteSpace(thumbnailFilename))
                        {
                            viewModel.ThumbnailUrl = $"/img/Videos/Thumb/{thumbnailFilename}";
                            viewModel.DetailsUrl = $"/Videos/Details/{videoId}";
                        }
                    }
                }
            }
            else if (comment.ReferenceType == "/Announcements" && !string.IsNullOrWhiteSpace(comment.ReferenceId))
            {
                // Validate reference ID is numeric
                if (int.TryParse(comment.ReferenceId, out var announcementId) && announcementId > 0)
                {
                    viewModel.DetailsUrl = $"/Announcements/Index#{announcementId}";
                }
            }
            else if (comment.ReferenceType == "Page" && !string.IsNullOrWhiteSpace(comment.ReferenceId))
            {
                // Validate reference ID is numeric
                if (int.TryParse(comment.ReferenceId, out var pageId) && pageId > 0)
                {
                    viewModel.DetailsUrl = $"/Page/Index/{pageId}";
                }
            }
            else if (!string.IsNullOrWhiteSpace(comment.ReferenceType) && comment.ReferenceType.StartsWith("/"))
            {
                // Only use reference type if it starts with / (relative URL)
                viewModel.DetailsUrl = comment.ReferenceType;
            }

            return viewModel;
        }

        public class CommentViewModel
        {
            public Comment Comment { get; set; } = null!;
            public ProfileEntity? Profile { get; set; }
            public string DisplayName { get; set; } = string.Empty;
            public string AvatarUrl { get; set; } = string.Empty;
            public string? ThumbnailUrl { get; set; }
            public string? DetailsUrl { get; set; }
        }
    }
}
