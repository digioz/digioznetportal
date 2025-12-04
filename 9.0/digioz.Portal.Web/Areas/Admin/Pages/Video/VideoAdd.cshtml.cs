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
using Microsoft.Extensions.Configuration;

namespace digioz.Portal.Web.Areas.Admin.Pages.Video
{
    public class VideoAddModel : PageModel
    {
        private readonly IVideoService _videoService;
        private readonly IVideoAlbumService _albumService;
        private readonly IUserHelper _userHelper;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _configuration;

        public VideoAddModel(IVideoService videoService, IVideoAlbumService albumService, IUserHelper userHelper, IWebHostEnvironment env, IConfiguration configuration)
        {
            _videoService = videoService;
            _albumService = albumService;
            _userHelper = userHelper;
            _env = env;
            _configuration = configuration;
        }

        [BindProperty] public digioz.Portal.Bo.Video Item { get; set; } = new digioz.Portal.Bo.Video { Visible = true, Approved = false, Timestamp = DateTime.UtcNow };
        [BindProperty] public IFormFile? ThumbnailFile { get; set; }
        [BindProperty] public IFormFile? VideoFile { get; set; }
        [BindProperty] public string? AssembledVideoPath { get; set; }
        public List<VideoAlbum> Albums { get; private set; } = new();
        public int ChunkSizeInMB { get; private set; }

        public void OnGet()
        {
            Albums = _albumService.GetAll().OrderBy(a => a.Name).ToList();
            ChunkSizeInMB = _configuration.GetValue<int>("ChunkedUpload:ChunkSizeInMB", 20);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Albums = _albumService.GetAll().OrderBy(a => a.Name).ToList();
            ChunkSizeInMB = _configuration.GetValue<int>("ChunkedUpload:ChunkSizeInMB", 20);
            
            if (!ModelState.IsValid) return Page();
            if (ThumbnailFile == null || ThumbnailFile.Length == 0)
            {
                ModelState.AddModelError("ThumbnailFile", "Please select a thumbnail image to upload.");
                return Page();
            }

            var webroot = _env.WebRootPath;
            
            // Check if video was uploaded via chunked upload or standard upload
            string videoExt;
            if (!string.IsNullOrEmpty(AssembledVideoPath))
            {
                var assembledPath = Path.Combine(webroot, "img", AssembledVideoPath.Replace("/", Path.DirectorySeparatorChar.ToString()));
                
                if (!System.IO.File.Exists(assembledPath))
                {
                    ModelState.AddModelError("VideoFile", "Assembled video file not found.");
                    return Page();
                }
                
                videoExt = Path.GetExtension(assembledPath).ToLowerInvariant();
            }
            else if (VideoFile != null && VideoFile.Length > 0)
            {
                videoExt = Path.GetExtension(VideoFile.FileName).ToLowerInvariant();
            }
            else
            {
                ModelState.AddModelError("VideoFile", "Please select a video to upload.");
                return Page();
            }

            var thumbExt = Path.GetExtension(ThumbnailFile.FileName).ToLowerInvariant();
            var imgAllowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tif", ".tiff" };
            if (!imgAllowed.Contains(thumbExt))
            {
                ModelState.AddModelError("ThumbnailFile", "Invalid image type.");
                return Page();
            }
            
            var vidAllowed = new[] { ".mp4", ".mov", ".avi", ".wmv", ".mkv", ".mpg", ".mpeg" };
            if (!vidAllowed.Contains(videoExt))
            {
                ModelState.AddModelError("VideoFile", "Invalid video type.");
                return Page();
            }

            var fullDir = Path.Combine(webroot, "img", "Videos", "Full");
            var thumbDir = Path.Combine(webroot, "img", "Videos", "Thumb");
            Directory.CreateDirectory(fullDir);
            Directory.CreateDirectory(thumbDir);

            var guid = Guid.NewGuid().ToString("N");
            var videoName = guid + videoExt;
            var thumbName = guid + thumbExt;
            var fullPath = Path.Combine(fullDir, videoName);
            var thumbPath = Path.Combine(thumbDir, thumbName);

            // Save video
            if (!string.IsNullOrEmpty(AssembledVideoPath))
            {
                // Move assembled video from chunks
                var assembledPath = Path.Combine(webroot, "img", AssembledVideoPath.Replace("/", Path.DirectorySeparatorChar.ToString()));
                if (System.IO.File.Exists(assembledPath))
                {
                    System.IO.File.Move(assembledPath, fullPath);
                    
                    // Cleanup upload directory
                    var uploadDir = Path.GetDirectoryName(assembledPath);
                    if (Directory.Exists(uploadDir))
                    {
                        try { Directory.Delete(uploadDir, true); } catch { }
                    }
                }
            }
            else
            {
                // Standard upload
                using (var fs = System.IO.File.Create(fullPath))
                {
                    await VideoFile.CopyToAsync(fs);
                }
            }

            // Save original thumbnail first
            var tempThumbPath = Path.Combine(fullDir, "temp_" + thumbName);
            using (var fs = System.IO.File.Create(tempThumbPath))
            {
                await ThumbnailFile.CopyToAsync(fs);
            }

            // Create thumbnail (crop150x150) using ImageSharp
            using (var image = SixLabors.ImageSharp.Image.Load(tempThumbPath))
            {
                ImageHelper.SaveImageWithCrop(image, 150, 150, thumbPath);
            }

            // Clean up temp file
            if (System.IO.File.Exists(tempThumbPath))
                System.IO.File.Delete(tempThumbPath);

            Item.Filename = videoName;
            Item.Thumbnail = thumbName;
            Item.Timestamp = DateTime.UtcNow;
            var email = User?.Identity?.Name;
            Item.UserId = !string.IsNullOrEmpty(email) ? _userHelper.GetUserIdByEmail(email) : null;

            _videoService.Add(Item);
            return RedirectToPage("/Video/VideoIndex", new { area = "Admin" });
        }
    }
}

