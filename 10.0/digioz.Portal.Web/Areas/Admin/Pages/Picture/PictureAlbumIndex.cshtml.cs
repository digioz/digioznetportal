using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using digioz.Portal.Dal.Services.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;
using digioz.Portal.Bo;

namespace digioz.Portal.Web.Areas.Admin.Pages.Picture
{
    public class PictureAlbumIndexModel : PageModel
    {
        private readonly IPictureAlbumService _albumService;
        private readonly IPictureService _pictureService;
        public PictureAlbumIndexModel(IPictureAlbumService albumService, IPictureService pictureService)
        {
            _albumService = albumService;
            _pictureService = pictureService;
        }
        public IReadOnlyList<PictureAlbum> Items { get; private set; } = Array.Empty<PictureAlbum>();
        [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
        [BindProperty(SupportsGet = true)] public int PageSize { get; set; } = 10;
        public int TotalCount { get; private set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / Math.Max(1, PageSize));
        public Dictionary<int, int> PhotoCounts { get; private set; } = new();
        public Dictionary<int, string?> LatestThumbs { get; private set; } = new();

        public void OnGet()
        {
            var all = _albumService.GetAll().OrderByDescending(a => a.Id).ToList();
            TotalCount = all.Count;
            var skip = (PageNumber - 1) * PageSize;
            Items = all.Skip(skip).Take(PageSize).ToList();

            // stats per album
            var pics = _pictureService.GetAll();
            PhotoCounts = pics.GroupBy(p => p.AlbumId).ToDictionary(g => g.Key, g => g.Count());
            LatestThumbs = pics
            .GroupBy(p => p.AlbumId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(p => p.Id).Select(p => p.Thumbnail).FirstOrDefault());
        }
    }
}
