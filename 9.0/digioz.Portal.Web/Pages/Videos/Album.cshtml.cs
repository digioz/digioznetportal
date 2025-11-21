using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.Utilities;

namespace digioz.Portal.Pages.Videos
{
    public class AlbumModel : PageModel
    {
        private readonly IVideoService _videoService;
        private readonly IVideoAlbumService _albumService;
        private readonly IUserHelper _userHelper;

        public AlbumModel(IVideoService videoService, IVideoAlbumService albumService, IUserHelper userHelper)
        {
            _videoService = videoService;
            _albumService = albumService;
            _userHelper = userHelper;
        }

        public VideoAlbum? Album { get; private set; }
        public IReadOnlyList<Video> Videos { get; private set; } = Array.Empty<Video>();

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
            var isAdmin = User?.IsInRole("Administrator") == true;

            // Use filtered query to only retrieve needed videos from database
            var allVideos = _videoService.GetFiltered(userId: userId, albumId: Id, isAdmin: isAdmin);

            TotalCount = allVideos.Count;

            if (PageNumber < 1) PageNumber = 1;
            if (PageSize < 1) PageSize = 12;

            var skip = (PageNumber - 1) * PageSize;
            Videos = allVideos.Skip(skip).Take(PageSize).ToList();

            return Page();
        }
    }
}
