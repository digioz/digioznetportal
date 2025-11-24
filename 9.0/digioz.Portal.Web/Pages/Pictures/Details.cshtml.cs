using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.Utilities;
using ProfileEntity = digioz.Portal.Bo.Profile;

namespace digioz.Portal.Pages.Pictures
{
    public class DetailsModel : PageModel
    {
        private readonly IPictureService _pictureService;
        private readonly IPictureAlbumService _albumService;
        private readonly IProfileService _profileService;
        private readonly IUserHelper _userHelper;
        private readonly ICommentsHelper _commentsHelper;

        public DetailsModel(IPictureService pictureService, IPictureAlbumService albumService, IProfileService profileService, IUserHelper userHelper, ICommentsHelper commentsHelper)
        {
            _pictureService = pictureService;
            _albumService = albumService;
            _profileService = profileService;
            _userHelper = userHelper;
            _commentsHelper = commentsHelper;
        }

        public Picture? Item { get; private set; }
        public PictureAlbum? Album { get; private set; }
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

            Item = _pictureService.Get(Id.Value);
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

            // Check if comments are enabled for this picture
            AllowComments = _commentsHelper.IsCommentsEnabledForPicture(Item.Id);

            return Page();
        }
        
        private void CalculateNavigation(string? userId, bool isAdmin)
        {
            if (Item == null) return;
            
            // Determine which pictures to navigate through
            // Use case-insensitive comparison with the sanitized value
            var pictures = Source?.Equals("album", StringComparison.OrdinalIgnoreCase) == true && AlbumId.HasValue
                ? _pictureService.GetFiltered(userId: userId, albumId: AlbumId.Value, isAdmin: isAdmin)
                : _pictureService.GetFiltered(userId: userId, isAdmin: isAdmin);
            
            if (pictures == null || pictures.Count == 0)
                return;
                
            // Find current item's index
            var currentIndex = -1;
            for (int i = 0; i < pictures.Count; i++)
            {
                if (pictures[i].Id == Item.Id)
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
                PreviousId = pictures[currentIndex - 1].Id;
            }
            else if (pictures.Count > 1)
            {
                // Loop to last item
                PreviousId = pictures[pictures.Count - 1].Id;
            }
            
            // Calculate next
            if (currentIndex < pictures.Count - 1)
            {
                NextId = pictures[currentIndex + 1].Id;
            }
            else if (pictures.Count > 1)
            {
                // Loop to first item
                NextId = pictures[0].Id;
            }
        }

        public IActionResult OnPostDelete(int id)
        {
            Item = _pictureService.Get(id);
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
                _pictureService.Delete(id);
                return RedirectToPage("List");
            }
            catch
            {
                StatusMessage = "Error deleting picture.";
                IsSuccess = false;
                return RedirectToPage("Details", new { id });
            }
        }
    }
}
