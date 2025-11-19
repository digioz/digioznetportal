using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.Utilities;

namespace digioz.Portal.Pages.Pictures
{
    public class DetailsModel : PageModel
    {
        private readonly IPictureService _pictureService;
        private readonly IPictureAlbumService _albumService;
        private readonly IUserHelper _userHelper;

        public DetailsModel(IPictureService pictureService, IPictureAlbumService albumService, IUserHelper userHelper)
        {
            _pictureService = pictureService;
            _albumService = albumService;
            _userHelper = userHelper;
        }

        public Picture? Item { get; private set; }
        public PictureAlbum? Album { get; private set; }
        public bool IsOwner { get; private set; }
        public string? StatusMessage { get; set; }
        public bool IsSuccess { get; set; }

        public IActionResult OnGet(int id)
        {
            Item = _pictureService.Get(id);
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
