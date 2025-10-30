using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.Utilities.Helpers;
using digioz.Portal.Utilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.Picture
{
    public class PictureEditModel : PageModel
    {
        private readonly IPictureService _pictureService;
        private readonly IPictureAlbumService _albumService;
        private readonly IUserHelper _userHelper;
        private readonly IWebHostEnvironment _env;

        public PictureEditModel(IPictureService pictureService, IPictureAlbumService albumService, IUserHelper userHelper, IWebHostEnvironment env)
        {
            _pictureService = pictureService;
            _albumService = albumService;
            _userHelper = userHelper;
            _env = env;
        }

        [BindProperty] public digioz.Portal.Bo.Picture Item { get; set; }
        [BindProperty] public IFormFile? NewFile { get; set; }
        public System.Collections.Generic.List<digioz.Portal.Bo.PictureAlbum> Albums { get; private set; } = new();

        public IActionResult OnGet(int id)
        {
            Item = _pictureService.Get(id);
            if (Item == null) return RedirectToPage("/Picture/PictureIndex", new { area = "Admin" });
            Albums = _albumService.GetAll().OrderBy(a => a.Name).ToList();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Albums = _albumService.GetAll().OrderBy(a => a.Name).ToList();
            if (!ModelState.IsValid) return Page();

            var existing = _pictureService.Get(Item.Id);
            if (existing == null)
            {
                return RedirectToPage("/Picture/PictureIndex", new { area = "Admin" });
            }

            existing.AlbumId = Item.AlbumId;
            existing.Description = Item.Description;
            existing.Approved = Item.Approved;
            existing.Visible = Item.Visible;

            if (NewFile != null && NewFile.Length > 0)
            {
                var ext = Path.GetExtension(NewFile.FileName).ToLowerInvariant();
                var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tif", ".tiff" };
                if (!allowed.Contains(ext))
                {
                    ModelState.AddModelError("NewFile", "Invalid image type.");
                    return Page();
                }

                var webroot = _env.WebRootPath;
                var fullDir = Path.Combine(webroot, "img", "Pictures", "Full");
                var thumbDir = Path.Combine(webroot, "img", "Pictures", "Thumb");
                Directory.CreateDirectory(fullDir);
                Directory.CreateDirectory(thumbDir);

                var newFileName = Guid.NewGuid().ToString("N") + ext;
                var fullPath = Path.Combine(fullDir, newFileName);
                var thumbPath = Path.Combine(thumbDir, newFileName);

                using (var fs = System.IO.File.Create(fullPath))
                {
                    await NewFile.CopyToAsync(fs);
                }

                using (var ms = new MemoryStream())
                {
                    await NewFile.CopyToAsync(ms);
                    ms.Position = 0;
                    using var image = System.Drawing.Image.FromStream(ms);
                    ImageHelper.SaveImageWithCrop(image, 150, 150, thumbPath);
                }

                TryDeleteExistingFiles(existing);

                existing.Filename = newFileName;
                existing.Thumbnail = newFileName;
            }

            existing.Timestamp = DateTime.UtcNow;
            var email = User?.Identity?.Name;
            existing.UserId = _userHelper.GetUserIdByEmail(email);

            _pictureService.Update(existing);
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
