using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using digioz.Portal.Dal.Services.Interfaces;

namespace digioz.Portal.Web.Areas.Admin.Pages.Picture
{
    public class PictureDeleteModel : PageModel
    {
        private readonly IPictureService _pictureService;
        private readonly IWebHostEnvironment _env;
        public PictureDeleteModel(IPictureService pictureService, IWebHostEnvironment env)
        {
            _pictureService = pictureService;
            _env = env;
        }

        [BindProperty(SupportsGet = true)] public int Id { get; set; }
        public digioz.Portal.Bo.Picture? Item { get; private set; }

        public IActionResult OnGet(int id)
        {
            Item = _pictureService.Get(id);
            if (Item == null) return RedirectToPage("/Picture/PictureIndex", new { area = "Admin" });
            return Page();
        }

        public IActionResult OnPost()
        {
            var item = _pictureService.Get(Id);
            if (item != null)
            {
                TryDeleteExistingFiles(item);
                _pictureService.Delete(Id);
            }
            return RedirectToPage("/Picture/PictureIndex", new { area = "Admin" });
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
