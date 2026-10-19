using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using digioz.Portal.Dal.Services.Interfaces;

namespace digioz.Portal.Web.Areas.Admin.Pages.Picture
{
    public class PictureAlbumDeleteModel : PageModel
    {
        private readonly IPictureAlbumService _albumService;
        private readonly IPictureService _pictureService;
        private readonly IWebHostEnvironment _env;
        public PictureAlbumDeleteModel(IPictureAlbumService albumService, IPictureService pictureService, IWebHostEnvironment env)
        { _albumService = albumService; _pictureService = pictureService; _env = env; }
        [BindProperty(SupportsGet = true)] public int Id { get; set; }
        public digioz.Portal.Bo.PictureAlbum? Item { get; private set; }
        public IActionResult OnGet(int id)
        {
            Item = _albumService.Get(id);
            if (Item == null) return RedirectToPage("/Picture/PictureAlbumIndex", new { area = "Admin" });
            return Page();
        }
        public IActionResult OnPost()
        {
            var pics = _pictureService.GetAll().Where(p => p.AlbumId == Id).ToList();
            foreach (var p in pics)
            {
                TryDeleteExistingFiles(p);
                _pictureService.Delete(p.Id);
            }
            _albumService.Delete(Id);
            return RedirectToPage("/Picture/PictureAlbumIndex", new { area = "Admin" });
        }
        private void TryDeleteExistingFiles(digioz.Portal.Bo.Picture pic)
        {
            try
            {
                var webroot = _env.WebRootPath;
                var fullPath = Path.Combine(webroot, "img", "Pictures", "Full", pic.Filename ?? "");
                var thumbPath = Path.Combine(webroot, "img", "Pictures", "Thumb", pic.Thumbnail ?? "");
                if (System.IO.File.Exists(fullPath)) System.IO.File.Delete(fullPath);
                if (System.IO.File.Exists(thumbPath)) System.IO.File.Delete(thumbPath);
            }
            catch { }
        }
    }
}
