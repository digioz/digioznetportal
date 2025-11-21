using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.RazorPages;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.Utilities;

namespace digioz.Portal.Pages.Videos
{
    public class ListModel : PageModel
    {
        private readonly IVideoService _videoService;
        private readonly IVideoAlbumService _albumService;
        private readonly IUserHelper _userHelper;

        public ListModel(IVideoService videoService, IVideoAlbumService albumService, IUserHelper userHelper)
        {
            _videoService = videoService;
            _albumService = albumService;
            _userHelper = userHelper;
        }

        public IReadOnlyList<Video> Videos { get; private set; } = Array.Empty<Video>();
        public Dictionary<int, string> AlbumNames { get; private set; } = new();

        [Microsoft.AspNetCore.Mvc.BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;
        
        [Microsoft.AspNetCore.Mvc.BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 12;

        public int TotalCount { get; private set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / Math.Max(1, PageSize));

        public void OnGet()
        {
            // Get current user ID if logged in
            var email = User?.Identity?.Name;
            var userId = !string.IsNullOrEmpty(email) ? _userHelper.GetUserIdByEmail(email) : null;
            var isAdmin = User?.IsInRole("Admin") == true;

            // Filter videos based on ownership and admin status
            var allVideos = _videoService.GetAll()
                .Where(v => 
                    // Show all videos to admins
                    isAdmin ||
                    // Show all their own videos to the owner (visible/hidden, approved/unapproved)
                    (userId != null && v.UserId == userId) ||
                    // Show only visible and approved videos to everyone else
                    (v.Visible && v.Approved)
                )
                .OrderByDescending(v => v.Timestamp)
                .ToList();

            TotalCount = allVideos.Count;
            
            if (PageNumber < 1) PageNumber = 1;
            if (PageSize < 1) PageSize = 12;
            
            var skip = (PageNumber - 1) * PageSize;
            Videos = allVideos.Skip(skip).Take(PageSize).ToList();

            // Map album names
            var albums = _albumService.GetAll();
            AlbumNames = albums.ToDictionary(a => a.Id, a => a.Name);
        }
    }
}