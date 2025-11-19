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

namespace digioz.Portal.Pages.Pictures {
    public class AddModel : PageModel {
        private readonly IPictureService _pictureService;
        private readonly IPictureAlbumService _albumService;
        private readonly IUserHelper _userHelper;
        private readonly IWebHostEnvironment _env;

        public AddModel(IPictureService pictureService, IPictureAlbumService albumService, IUserHelper userHelper, IWebHostEnvironment env) {
            _pictureService = pictureService;
            _albumService = albumService;
            _userHelper = userHelper;
            _env = env;
        }

        [BindProperty]
        public int AlbumId { get; set; }

        [BindProperty]
        public List<IFormFile>? Files { get; set; } = new();

        [BindProperty]
        public List<string>? Descriptions { get; set; } = new();

        public List<PictureAlbum> Albums { get; private set; } = new();
        public string? StatusMessage { get; set; }
        public bool IsSuccess { get; set; }

        public void OnGet() {
            Albums = _albumService.GetAll().OrderBy(a => a.Name).ToList();
        }

        public async Task<IActionResult> OnPostAsync() {
            Albums = _albumService.GetAll().OrderBy(a => a.Name).ToList();

            if (AlbumId <= 0) {
                ModelState.AddModelError("AlbumId", "Please select an album.");
                StatusMessage = "Please select an album.";
                IsSuccess = false;
                return Page();
            }

            if (Files == null || Files.Count == 0) {
                ModelState.AddModelError("Files", "Please select at least one image to upload.");
                StatusMessage = "Please select at least one image to upload.";
                IsSuccess = false;
                return Page();
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tif", ".tiff" };
            var webroot = _env.WebRootPath;
            var fullDir = Path.Combine(webroot, "img", "Pictures", "Full");
            var thumbDir = Path.Combine(webroot, "img", "Pictures", "Thumb");
            Directory.CreateDirectory(fullDir);
            Directory.CreateDirectory(thumbDir);

            var email = User?.Identity?.Name;
            var userId = !string.IsNullOrEmpty(email) ? _userHelper.GetUserIdByEmail(email) : null;

            int successCount = 0;
            int errorCount = 0;

            for (int i = 0; i < Files.Count; i++) {
                var file = Files[i];
                var description = (Descriptions != null && i < Descriptions.Count) ? Descriptions[i] : string.Empty;

                if (file == null || file.Length == 0)
                    continue;

                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(ext)) {
                    errorCount++;
                    continue;
                }

                try {
                    var fileName = Guid.NewGuid().ToString("N") + ext;
                    var fullPath = Path.Combine(fullDir, fileName);
                    var thumbPath = Path.Combine(thumbDir, fileName);

                    // Save original
                    using (var fs = System.IO.File.Create(fullPath)) {
                        await file.CopyToAsync(fs);
                    }

                    // Create thumbnail (max150x150 crop) using ImageSharp
                    using (var image = SixLabors.ImageSharp.Image.Load(fullPath)) {
                        ImageHelper.SaveImageWithCrop(image, 150, 150, thumbPath);
                    }

                    var picture = new Picture {
                        Filename = fileName,
                        Thumbnail = fileName,
                        Description = description,
                        AlbumId = AlbumId,
                        UserId = userId,
                        Visible = false,
                        Approved = false,
                        Timestamp = DateTime.UtcNow
                    };

                    _pictureService.Add(picture);
                    successCount++;
                } catch {
                    errorCount++;
                }
            }

            if (successCount > 0) {
                StatusMessage = $"Successfully uploaded {successCount} picture(s).";
                IsSuccess = true;
                if (errorCount > 0)
                    StatusMessage += $" {errorCount} picture(s) failed to upload.";
            } else {
                StatusMessage = $"Failed to upload pictures. {errorCount} error(s).";
                IsSuccess = false;
            }

            return Page();
        }
    }
}