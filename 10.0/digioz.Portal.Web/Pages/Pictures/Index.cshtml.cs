using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.RazorPages;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.Utilities;

namespace digioz.Portal.Pages.Pictures
{
    public class IndexModel : PageModel
    {
        private readonly IPictureAlbumService _albumService;
        private readonly IPictureService _pictureService;
        private readonly IUserHelper _userHelper;

        public IndexModel(IPictureAlbumService albumService, IPictureService pictureService, IUserHelper userHelper)
        {
            _albumService = albumService;
            _pictureService = pictureService;
            _userHelper = userHelper;
        }

        public IReadOnlyList<PictureAlbum> Albums { get; private set; } = Array.Empty<PictureAlbum>();
        public Dictionary<int, Picture> LatestPictureByAlbum { get; private set; } = new();
        
        [Microsoft.AspNetCore.Mvc.BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;
        
        [Microsoft.AspNetCore.Mvc.BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 9;

        public int TotalCount { get; private set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / Math.Max(1, PageSize));

        public void OnGet()
        {
            var allAlbums = _albumService.GetAll()
                .Where(a => a.Visible)
                .OrderByDescending(a => a.Timestamp)
                .ToList();

            TotalCount = allAlbums.Count;
            
            if (PageNumber < 1) PageNumber = 1;
            if (PageSize < 1) PageSize = 9;
            
            var skip = (PageNumber - 1) * PageSize;
            Albums = allAlbums.Skip(skip).Take(PageSize).ToList();

            // Get current user ID if logged in
            var email = User?.Identity?.Name;
            var userId = !string.IsNullOrEmpty(email) ? _userHelper.GetUserIdByEmail(email) : null;

            // Get all pictures to find latest visible picture per album
            var allPictures = _pictureService.GetAll()
                .Where(p => p.Visible && (
                    // Show approved pictures to everyone
                    p.Approved || 
                    // Show unapproved pictures only to their owner
                    (userId != null && p.UserId == userId)
                ))
                .OrderByDescending(p => p.Timestamp)
                .ToList();

            // For each album, get the latest visible picture that meets the criteria
            foreach (var album in Albums)
            {
                var latestPic = allPictures.FirstOrDefault(p => p.AlbumId == album.Id);
                if (latestPic != null)
                {
                    LatestPictureByAlbum[album.Id] = latestPic;
                }
            }
        }
    }
}