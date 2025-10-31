using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.Video
{
    public class VideoAlbumAddModel : PageModel
    {
        private readonly IVideoAlbumService _albumService;
        private readonly IVideoService _videoService;
        private readonly digioz.Portal.Utilities.IUserHelper _userHelper;
        private readonly IWebHostEnvironment _env;
        public VideoAlbumAddModel(IVideoAlbumService albumService, IVideoService videoService, digioz.Portal.Utilities.IUserHelper userHelper, IWebHostEnvironment env)
        { _albumService = albumService; _videoService = videoService; _userHelper = userHelper; _env = env; }
        [BindProperty] public digioz.Portal.Bo.VideoAlbum Item { get; set; } = new digioz.Portal.Bo.VideoAlbum { Visible = true, Approved = false, Timestamp = DateTime.UtcNow };
        [BindProperty] public List<IFormFile> VideoFiles { get; set; } = new();
        [BindProperty] public List<IFormFile> ThumbnailFiles { get; set; } = new();
        public void OnGet() { }
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();
            Item.Timestamp = DateTime.UtcNow;
            _albumService.Add(Item);
            var webroot = _env.WebRootPath;
            var fullDir = Path.Combine(webroot, "img", "Videos", "Full");
            var thumbDir = Path.Combine(webroot, "img", "Videos", "Thumb");
            Directory.CreateDirectory(fullDir);
            Directory.CreateDirectory(thumbDir);
            var vidAllowed = new[] { ".mp4", ".mov", ".avi", ".wmv", ".mkv", ".mpg", ".mpeg" };
            var imgAllowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tif", ".tiff" };
            for (int i = 0; i < VideoFiles.Count; i++)
            {
                var v = VideoFiles[i];
                if (v == null || v.Length == 0) continue;
                var vidExt = Path.GetExtension(v.FileName).ToLowerInvariant();
                if (!vidAllowed.Contains(vidExt)) continue;
                var guid = Guid.NewGuid().ToString("N");
                var videoName = guid + vidExt;
                var fullPath = Path.Combine(fullDir, videoName);
                using (var fs = System.IO.File.Create(fullPath))
                {
                    await v.CopyToAsync(fs);
                }
                string? thumbName = null;
                if (i < ThumbnailFiles.Count && ThumbnailFiles[i] != null && ThumbnailFiles[i].Length > 0)
                {
                    var t = ThumbnailFiles[i];
                    var tExt = Path.GetExtension(t.FileName).ToLowerInvariant();
                    if (imgAllowed.Contains(tExt))
                    {
                        thumbName = guid + tExt;
                        var thumbPath = Path.Combine(thumbDir, thumbName);

                        // Save temp file first
                        var tempPath = Path.Combine(thumbDir, "temp_" + thumbName);
                        using (var fs = System.IO.File.Create(tempPath))
                        {
                            await t.CopyToAsync(fs);
                        }

                        // Create thumbnail using ImageSharp
                        using (var image = SixLabors.ImageSharp.Image.Load(tempPath))
                        {
                            digioz.Portal.Utilities.Helpers.ImageHelper.SaveImageWithCrop(image, 150, 150, thumbPath);
                        }

                        // Clean up temp file
                        if (System.IO.File.Exists(tempPath))
                            System.IO.File.Delete(tempPath);
                    }
                }
                var userId = _userHelper.GetUserIdByEmail(User?.Identity?.Name);
                var video = new digioz.Portal.Bo.Video
                {
                    AlbumId = Item.Id,
                    Filename = videoName,
                    Thumbnail = thumbName ?? string.Empty,
                    Description = string.Empty,
                    Approved = false,
                    Visible = true,
                    Timestamp = DateTime.UtcNow,
                    UserId = userId
                };
                _videoService.Add(video);
            }
            return RedirectToPage("/Video/VideoAlbumIndex", new { area = "Admin" });
        }
    }
}
