using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.RazorPages;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.Utilities;

namespace digioz.Portal.Pages.Videos
{
    public class IndexModel : PageModel
    {
        private readonly IVideoAlbumService _albumService;
        private readonly IVideoService _videoService;
        private readonly IUserHelper _userHelper;

        public IndexModel(IVideoAlbumService albumService, IVideoService videoService, IUserHelper userHelper)
        {
            _albumService = albumService;
            _videoService = videoService;
            _userHelper = userHelper;
        }

        public IReadOnlyList<VideoAlbum> Albums { get; private set; } = Array.Empty<VideoAlbum>();
        public Dictionary<int, Video> LatestVideoByAlbum { get; private set; } = new();
        
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

            // Get all videos to find latest visible video per album
            // Show videos that are either:
            // 1. Visible and Approved (for everyone)
            // 2. Owned by current user regardless of Visible/Approved status
            var allVideos = _videoService.GetAll()
                .Where(v => (v.Visible && v.Approved) || (userId != null && v.UserId == userId))
                .OrderByDescending(v => v.Timestamp)
                .ToList();

            // For each album, get the latest video that meets the criteria
            foreach (var album in Albums)
            {
                var latestVideo = allVideos.FirstOrDefault(v => v.AlbumId == album.Id);
                if (latestVideo != null)
                {
                    LatestVideoByAlbum[album.Id] = latestVideo;
                }
            }
        }
    }
}