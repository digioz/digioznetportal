using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.Utilities;

namespace digioz.Portal.Pages.Videos
{
    public class DetailsModel : PageModel
    {
        private readonly IVideoService _videoService;
        private readonly IVideoAlbumService _albumService;
        private readonly IUserHelper _userHelper;

        public DetailsModel(IVideoService videoService, IVideoAlbumService albumService, IUserHelper userHelper)
        {
            _videoService = videoService;
            _albumService = albumService;
            _userHelper = userHelper;
        }

        public Video? Item { get; private set; }
        public VideoAlbum? Album { get; private set; }
        public bool IsOwner { get; private set; }
        public string? StatusMessage { get; set; }
        public bool IsSuccess { get; set; }

        public IActionResult OnGet(int id)
        {
            Item = _videoService.Get(id);
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
            return Page();
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
