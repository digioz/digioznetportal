using System;
using System.Collections.Generic;
using System.Linq;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.Video
{
    public class VideoIndexModel : PageModel
    {
        private readonly IVideoService _videoService;
        private readonly IVideoAlbumService _albumService;
        public VideoIndexModel(IVideoService videoService, IVideoAlbumService albumService)
        {
            _videoService = videoService;
            _albumService = albumService;
        }
        public IReadOnlyList<digioz.Portal.Bo.Video> Items { get; private set; } = Array.Empty<digioz.Portal.Bo.Video>();
        public Dictionary<int, string> AlbumNames { get; private set; } = new();
        [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
        [BindProperty(SupportsGet = true)] public int PageSize { get; set; } = 10;
        public int TotalCount { get; private set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / Math.Max(1, PageSize));
        public void OnGet()
        {
            var all = _videoService.GetAll().OrderByDescending(v => v.Id).ToList();
            TotalCount = all.Count;
            if (PageNumber < 1) PageNumber = 1;
            if (PageSize < 1) PageSize = 10;
            var skip = (PageNumber - 1) * PageSize;
            Items = all.Skip(skip).Take(PageSize).ToList();
            var albums = _albumService.GetAll();
            AlbumNames = albums.ToDictionary(a => a.Id, a => a.Name);
        }

        public IActionResult OnPostApprove(int id)
        {
            var video = _videoService.Get(id);
            if (video == null)
            {
                return RedirectToPage();
            }

            // Set approved and visible to true, preserving original UserId and Timestamp
            video.Approved = true;
            video.Visible = true;

            _videoService.Update(video);

            return RedirectToPage(new { pageNumber = PageNumber, pageSize = PageSize });
        }
    }
}
