using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.RazorPages;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.Utilities;

namespace digioz.Portal.Pages.Pictures
{
    public class ListModel : PageModel
    {
        private readonly IPictureService _pictureService;
        private readonly IPictureAlbumService _albumService;
        private readonly IUserHelper _userHelper;

        public ListModel(IPictureService pictureService, IPictureAlbumService albumService, IUserHelper userHelper)
        {
            _pictureService = pictureService;
            _albumService = albumService;
            _userHelper = userHelper;
        }

        public IReadOnlyList<Picture> Pictures { get; private set; } = Array.Empty<Picture>();
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
            var isAdmin = User?.IsInRole("Administrator") == true;

            // Filter pictures based on ownership and admin status
            var allPictures = _pictureService.GetAll()
                .Where(p => 
                    // Show all pictures to admins
                    isAdmin ||
                    // Show all their own pictures to the owner (visible/hidden, approved/unapproved)
                    (userId != null && p.UserId == userId) ||
                    // Show only visible and approved pictures to everyone else
                    (p.Visible && p.Approved)
                )
                .OrderByDescending(p => p.Timestamp)
                .ToList();

            TotalCount = allPictures.Count;
            
            if (PageNumber < 1) PageNumber = 1;
            if (PageSize < 1) PageSize = 12;
            
            var skip = (PageNumber - 1) * PageSize;
            Pictures = allPictures.Skip(skip).Take(PageSize).ToList();

            // Map album names
            var albums = _albumService.GetAll();
            AlbumNames = albums.ToDictionary(a => a.Id, a => a.Name);
        }
    }
}