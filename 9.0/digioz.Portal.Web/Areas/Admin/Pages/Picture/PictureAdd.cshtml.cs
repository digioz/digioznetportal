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
    public class PictureAddModel : PageModel
    {
        private readonly IPictureService _pictureService;
        private readonly IPictureAlbumService _albumService;
        private readonly IUserHelper _userHelper;
        private readonly IWebHostEnvironment _env;

        public PictureAddModel(IPictureService pictureService, IPictureAlbumService albumService, IUserHelper userHelper, IWebHostEnvironment env)
        {
            _pictureService = pictureService;
            _albumService = albumService;
            _userHelper = userHelper;
            _env = env;
        }

        [BindProperty] public digioz.Portal.Bo.Picture Item { get; set; } = new digioz.Portal.Bo.Picture { Visible = true, Approved = false, Timestamp = DateTime.UtcNow };
        [BindProperty] public IFormFile? File { get; set; }
        public List<PictureAlbum> Albums { get; private set; } = new();

        public void OnGet()
        {
            Albums = _albumService.GetAll().OrderBy(a => a.Name).ToList();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Albums = _albumService.GetAll().OrderBy(a => a.Name).ToList();
            if (!ModelState.IsValid) return Page();
            if (File == null || File.Length == 0)
            {
                ModelState.AddModelError("File", "Please select an image file to upload.");
                return Page();
            }

            var ext = Path.GetExtension(File.FileName).ToLowerInvariant();
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tif", ".tiff" };
            if (!allowed.Contains(ext))
            {
                ModelState.AddModelError("File", "Invalid image type. Allowed: jpg, png, gif, bmp, tif, tiff.");
                return Page();
            }

            var webroot = _env.WebRootPath;
            var fullDir = Path.Combine(webroot, "img", "Pictures", "Full");
            var thumbDir = Path.Combine(webroot, "img", "Pictures", "Thumb");
            Directory.CreateDirectory(fullDir);
            Directory.CreateDirectory(thumbDir);

            var fileName = Guid.NewGuid().ToString("N") + ext;
            var fullPath = Path.Combine(fullDir, fileName);
            var thumbPath = Path.Combine(thumbDir, fileName);

            // Save original
            using (var fs = System.IO.File.Create(fullPath))
            {
                await File.CopyToAsync(fs);
            }

            // Create thumbnail (max150x150 crop)
            using (var ms = new MemoryStream())
            {
                await File.CopyToAsync(ms);
                ms.Position = 0;
                using var image = System.Drawing.Image.FromStream(ms);
                ImageHelper.SaveImageWithCrop(image, 150, 150, thumbPath);
            }

            Item.Filename = fileName;
            Item.Thumbnail = fileName;
            Item.Timestamp = DateTime.UtcNow;
            var email = User?.Identity?.Name;
            Item.UserId = _userHelper.GetUserIdByEmail(email);

            _pictureService.Add(Item);
            return RedirectToPage("/Picture/PictureIndex", new { area = "Admin" });
        }
    }
}
