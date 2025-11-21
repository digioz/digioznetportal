using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.Utilities;

namespace digioz.Portal.Pages.Pictures
{
    public class AlbumModel : PageModel
    {
        private readonly IPictureService _pictureService;
        private readonly IPictureAlbumService _albumService;
        private readonly IUserHelper _userHelper;

        public AlbumModel(IPictureService pictureService, IPictureAlbumService albumService, IUserHelper userHelper)
        {
            _pictureService = pictureService;
            _albumService = albumService;
            _userHelper = userHelper;
        }

        public PictureAlbum? Album { get; private set; }
        public IReadOnlyList<Picture> Pictures { get; private set; } = Array.Empty<Picture>();

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 12;

        public int TotalCount { get; private set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / Math.Max(1, PageSize));

        public IActionResult OnGet()
        {
            Album = _albumService.Get(Id);
            if (Album == null)
                return NotFound();

            // Get current user ID if logged in
            var email = User?.Identity?.Name;
            var userId = !string.IsNullOrEmpty(email) ? _userHelper.GetUserIdByEmail(email) : null;
            var isAdmin = User?.IsInRole("Admin") == true;

            // Use filtered query to only retrieve needed pictures from database
            var allPictures = _pictureService.GetFiltered(userId: userId, albumId: Id, isAdmin: isAdmin);

            TotalCount = allPictures.Count;

            if (PageNumber < 1) PageNumber = 1;
            if (PageSize < 1) PageSize = 12;

            var skip = (PageNumber - 1) * PageSize;
            Pictures = allPictures.Skip(skip).Take(PageSize).ToList();

            return Page();
        }
    }
}
