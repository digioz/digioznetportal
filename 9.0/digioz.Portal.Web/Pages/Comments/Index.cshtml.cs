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

            // Build view models with additional data
            Comments = pagedComments.Select(c => CreateCommentViewModel(c)).ToList();
        }

        private CommentViewModel CreateCommentViewModel(Comment comment)
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

            // Get user profile information
            if (!string.IsNullOrEmpty(comment.UserId))
            {
                viewModel.Profile = _profileService.GetByUserId(comment.UserId);
                if (viewModel.Profile != null)
                {
                    // Use DisplayName if available, otherwise construct from first/last name
                    if (!string.IsNullOrWhiteSpace(viewModel.Profile.DisplayName))
                    {
                        viewModel.DisplayName = viewModel.Profile.DisplayName;
                    }
                    else if (!string.IsNullOrWhiteSpace(viewModel.Profile.FirstName) || !string.IsNullOrWhiteSpace(viewModel.Profile.LastName))
                    {
                        viewModel.DisplayName = $"{viewModel.Profile.FirstName} {viewModel.Profile.LastName}".Trim();
                    }

                    // Get avatar
                    if (!string.IsNullOrWhiteSpace(viewModel.Profile.Avatar))
                    {
                        viewModel.AvatarUrl = $"/img/avatar/thumb/{viewModel.Profile.Avatar}";
                    }
                }
            }

            // Determine media type and get thumbnail/details URL
            if (comment.ReferenceType == "/Pictures/Details" && !string.IsNullOrWhiteSpace(comment.ReferenceId))
            {
                if (int.TryParse(comment.ReferenceId, out var pictureId))
                {
                    var picture = _pictureService.Get(pictureId);
                    if (picture != null)
                    {
                        viewModel.ThumbnailUrl = $"/img/Pictures/Thumb/{picture.Thumbnail}";
                        viewModel.DetailsUrl = $"/Pictures/Details/{pictureId}";
                    }
                }
            }
            else if (comment.ReferenceType == "/Videos/Details" && !string.IsNullOrWhiteSpace(comment.ReferenceId))
            {
                if (int.TryParse(comment.ReferenceId, out var videoId))
                {
                    var video = _videoService.Get(videoId);
                    if (video != null)
                    {
                        viewModel.ThumbnailUrl = $"/img/Videos/Thumb/{video.Thumbnail}";
                        viewModel.DetailsUrl = $"/Videos/Details/{videoId}";
                    }
                }
            }
            else if (comment.ReferenceType == "/Announcements" && !string.IsNullOrWhiteSpace(comment.ReferenceId))
            {
                viewModel.DetailsUrl = $"/Announcements/Index#{comment.ReferenceId}";
            }
            else if (comment.ReferenceType == "Page" && !string.IsNullOrWhiteSpace(comment.ReferenceId))
            {
                viewModel.DetailsUrl = $"/Page/Index/{comment.ReferenceId}";
            }
            else
            {
                // Default to the reference type as the URL
                viewModel.DetailsUrl = comment.ReferenceType ?? "/";
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
