using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.Utilities;
using ProfileEntity = digioz.Portal.Bo.Profile;

namespace digioz.Portal.Pages.Videos
{
    public class DetailsModel : PageModel
    {
        private readonly IVideoService _videoService;
        private readonly IVideoAlbumService _albumService;
        private readonly IProfileService _profileService;
        private readonly IUserHelper _userHelper;
        private readonly ICommentsHelper _commentsHelper;

        public DetailsModel(IVideoService videoService, IVideoAlbumService albumService, IProfileService profileService, IUserHelper userHelper, ICommentsHelper commentsHelper)
        {
            _videoService = videoService;
            _albumService = albumService;
            _profileService = profileService;
            _userHelper = userHelper;
            _commentsHelper = commentsHelper;
        }

        public Video? Item { get; private set; }
        public VideoAlbum? Album { get; private set; }
        public ProfileEntity? UploaderProfile { get; private set; }
        public bool IsOwner { get; private set; }
        public string? StatusMessage { get; set; }
        public bool IsSuccess { get; set; }
        public bool AllowComments { get; set; }
        
        private string? _source;
        
        [BindProperty(SupportsGet = true)]
        public string? Source 
        { 
            get => _source;
            set => _source = StringUtils.SanitizeMediaSource(value);
        }
        
        [BindProperty(SupportsGet = true)]
        public int? AlbumId { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public int? Id { get; set; }
        
        public int? PreviousId { get; private set; }
        public int? NextId { get; private set; }
        
        /// <summary>
        /// Computed property to check if the source is an album view
        /// </summary>
        public bool IsAlbumSource => Source?.Equals("album", StringComparison.OrdinalIgnoreCase) == true && AlbumId.HasValue;

        public IActionResult OnGet()
        {
            if (!Id.HasValue)
                return NotFound();

            Item = _videoService.Get(Id.Value);
            if (Item == null)
                return NotFound();

            // Only show if visible and approved, or if owner/admin
            var email = User?.Identity?.Name;
            var userId = !string.IsNullOrEmpty(email) ? _userHelper.GetUserIdByEmail(email) : null;
            IsOwner = Item.UserId == userId;

            bool isAdmin = User?.IsInRole("Admin") == true;
            if (!Item.Visible || !Item.Approved)
            {
                if (!IsOwner && !isAdmin)
                    return Forbid();
            }

            // Increment view count
            _videoService.IncrementViews(Id.Value);

            Album = _albumService.Get(Item.AlbumId);
            
            // Get uploader profile
            if (!string.IsNullOrEmpty(Item.UserId))
            {
                UploaderProfile = _profileService.GetByUserId(Item.UserId);
            }
            
            // If AlbumId is not provided but source is album, use the current item's album
            if (Source?.Equals("album", StringComparison.OrdinalIgnoreCase) == true && !AlbumId.HasValue)
            {
                AlbumId = Item.AlbumId;
            }
            
            // Calculate previous and next based on source
            CalculateNavigation(userId, isAdmin);

            // Check if comments are enabled for this video
            AllowComments = _commentsHelper.IsCommentsEnabledForVideo(Item.Id);

            return Page();
        }
        
        private void CalculateNavigation(string? userId, bool isAdmin)
        {
            if (Item == null) return;
            
            // Determine which videos to navigate through
            // Use case-insensitive comparison with the sanitized value
            var videos = Source?.Equals("album", StringComparison.OrdinalIgnoreCase) == true && AlbumId.HasValue
                ? _videoService.GetFiltered(userId: userId, albumId: AlbumId.Value, isAdmin: isAdmin)
                : _videoService.GetFiltered(userId: userId, isAdmin: isAdmin);
            
            if (videos == null || videos.Count == 0)
                return;
                
            // Find current item's index
            var currentIndex = -1;
            for (int i = 0; i < videos.Count; i++)
            {
                if (videos[i].Id == Item.Id)
                {
                    currentIndex = i;
                    break;
                }
            }
            
            if (currentIndex == -1)
                return; // Current item not found in the list
            
            // Calculate previous
            if (currentIndex > 0)
            {
                PreviousId = videos[currentIndex - 1].Id;
            }
            else if (videos.Count > 1)
            {
                // Loop to last item
                PreviousId = videos[videos.Count - 1].Id;
            }
            
            // Calculate next
            if (currentIndex < videos.Count - 1)
            {
                NextId = videos[currentIndex + 1].Id;
            }
            else if (videos.Count > 1)
            {
                // Loop to first item
                NextId = videos[0].Id;
            }
        }

        public IActionResult OnPostDelete(int id)
        {
            Item = _videoService.Get(id);
            if (Item == null)
                return NotFound();

            var email = User?.Identity?.Name;
            var userId = !string.IsNullOrEmpty(email) ? _userHelper.GetUserIdByEmail(email) : null;
            IsOwner = Item.UserId == userId;

            bool isAdmin = User?.IsInRole("Admin") == true;
            if (!IsOwner && !isAdmin)
                return Forbid();

            try
            {
                _videoService.Delete(id);
                return RedirectToPage("List");
            }
            catch
            {
                StatusMessage = "Error deleting video.";
                IsSuccess = false;
                return RedirectToPage("Details", new { id });
            }
        }
    }
}
