using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.Utilities;
using digioz.Portal.Utilities.Helpers;
using digioz.Portal.Bo;

namespace digioz.Portal.Web.Areas.Admin.Pages.Picture
{
    public class PictureAlbumAddModel : PageModel
    {
        private readonly IPictureAlbumService _albumService;
        private readonly IPictureService _pictureService;
        private readonly IUserHelper _userHelper;
        private readonly IWebHostEnvironment _env;
        public PictureAlbumAddModel(IPictureAlbumService albumService, IPictureService pictureService, IUserHelper userHelper, IWebHostEnvironment env)
        {
            _albumService = albumService;
            _pictureService = pictureService;
            _userHelper = userHelper;
            _env = env;
        }
        [BindProperty] public PictureAlbum Item { get; set; } = new PictureAlbum { Visible = true, Approved = false, Timestamp = DateTime.UtcNow };
        [BindProperty] public List<IFormFile> Files { get; set; } = new();

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();
            Item.Timestamp = DateTime.UtcNow;
            _albumService.Add(Item);

            if (Files != null && Files.Any())
            {
                var webroot = _env.WebRootPath;
                var fullDir = Path.Combine(webroot, "img", "Pictures", "Full");
                var thumbDir = Path.Combine(webroot, "img", "Pictures", "Thumb");
                Directory.CreateDirectory(fullDir);
                Directory.CreateDirectory(thumbDir);
                var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tif", ".tiff" };
                var email = User?.Identity?.Name;
                var userId = _userHelper.GetUserIdByEmail(email);
                foreach (var file in Files)
                {
                    if (file == null || file.Length == 0) continue;
                    var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                    if (!allowed.Contains(ext)) continue;
                    var fileName = Guid.NewGuid().ToString("N") + ext;
                    var fullPath = Path.Combine(fullDir, fileName);
                    var thumbPath = Path.Combine(thumbDir, fileName);
                    using (var fs = System.IO.File.Create(fullPath))
                    {
                        await file.CopyToAsync(fs);
                    }
                    // Create thumbnail using ImageSharp-based helper
                    using (var image = SixLabors.ImageSharp.Image.Load(fullPath))
                    {
                        ImageHelper.SaveImageWithCrop(image, 150, 150, thumbPath);
                    }
                    var pic = new digioz.Portal.Bo.Picture
                    {
                        AlbumId = Item.Id,
                        Filename = fileName,
                        Thumbnail = fileName,
                        Description = string.Empty,
                        Approved = false,
                        Visible = true,
                        Timestamp = DateTime.UtcNow,
                        UserId = userId
                    };
                    _pictureService.Add(pic);
                }
            }
            return RedirectToPage("/Picture/PictureAlbumIndex", new { area = "Admin" });
        }
    }
}
