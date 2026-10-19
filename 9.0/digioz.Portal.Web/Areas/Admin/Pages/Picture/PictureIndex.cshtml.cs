using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.Utilities;
using digioz.Portal.Utilities.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.Picture
{
    public class PictureIndexModel : PageModel
    {
        private readonly IPictureService _pictureService;
        private readonly IPictureAlbumService _albumService;

        public PictureIndexModel(IPictureService pictureService, IPictureAlbumService albumService)
        {
            _pictureService = pictureService;
            _albumService = albumService;
        }

        public IReadOnlyList<digioz.Portal.Bo.Picture> Items { get; private set; } = Array.Empty<digioz.Portal.Bo.Picture>();
        public Dictionary<int, string> AlbumNames { get; private set; } = new();
        [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
        [BindProperty(SupportsGet = true)] public int PageSize { get; set; } = 10;
        public int TotalCount { get; private set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / Math.Max(1, PageSize));

        public void OnGet()
        {
            var all = _pictureService.GetAll().OrderByDescending(p => p.Id).ToList();
            TotalCount = all.Count;
            if (PageNumber < 1) PageNumber = 1;
            if (PageSize < 1) PageSize = 10;
            var skip = (PageNumber - 1) * PageSize;
            Items = all.Skip(skip).Take(PageSize).ToList();

            // Map album names
            var albums = _albumService.GetAll();
            AlbumNames = albums.ToDictionary(a => a.Id, a => a.Name);
        }

        public IActionResult OnPostApprove(int id)
        {
            var picture = _pictureService.Get(id);
            if (picture == null)
            {
                return RedirectToPage();
            }

            // Set approved and visible to true, preserving original UserId and Timestamp
            picture.Approved = true;
            picture.Visible = true;

            _pictureService.Update(picture);

            return RedirectToPage(new { pageNumber = PageNumber, pageSize = PageSize });
        }
    }
}
