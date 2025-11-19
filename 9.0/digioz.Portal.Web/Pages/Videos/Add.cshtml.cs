using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.Utilities.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using digioz.Portal.Utilities;

namespace digioz.Portal.Pages.Videos
{
    public class AddModel : PageModel
    {
        private readonly IVideoService _videoService;
        private readonly IVideoAlbumService _albumService;
        private readonly IUserHelper _userHelper;
        private readonly IWebHostEnvironment _env;

        public AddModel(IVideoService videoService, IVideoAlbumService albumService, IUserHelper userHelper, IWebHostEnvironment env)
        {
            _videoService = videoService;
            _albumService = albumService;
            _userHelper = userHelper;
            _env = env;
        }

        [BindProperty]
        public int AlbumId { get; set; }

        [BindProperty]
        public IFormFile ThumbnailFile { get; set; }

        [BindProperty]
        public IFormFile VideoFile { get; set; }

        [BindProperty]
        public string Description { get; set; }

        public List<VideoAlbum> Albums { get; private set; } = new();
        public string StatusMessage { get; set; }
        public bool IsSuccess { get; set; }

        public void OnGet()
        {
            Albums = _albumService.GetAll().OrderBy(a => a.Name).ToList();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Albums = _albumService.GetAll().OrderBy(a => a.Name).ToList();

            if (AlbumId <= 0)
            {
                ModelState.AddModelError("AlbumId", "Please select an album.");
                StatusMessage = "Please select an album.";
                IsSuccess = false;
                return Page();
            }

            if (ThumbnailFile == null || ThumbnailFile.Length == 0)
            {
                ModelState.AddModelError("ThumbnailFile", "Please select a thumbnail image.");
                StatusMessage = "Please select a thumbnail image.";
                IsSuccess = false;
                return Page();
            }

            if (VideoFile == null || VideoFile.Length == 0)
            {
                ModelState.AddModelError("VideoFile", "Please select a video file.");
                StatusMessage = "Please select a video file.";
                IsSuccess = false;
                return Page();
            }

            try
            {
                // Validate thumbnail
                var thumbExt = Path.GetExtension(ThumbnailFile.FileName).ToLowerInvariant();
                var allowedImageExt = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tif", ".tiff" };
                
                if (!allowedImageExt.Contains(thumbExt))
                {
                    ModelState.AddModelError("ThumbnailFile", "Invalid image type. Allowed: jpg, png, gif, bmp, tif, tiff.");
                    StatusMessage = "Invalid thumbnail image type.";
                    IsSuccess = false;
                    return Page();
                }

                // Validate video
                var videoExt = Path.GetExtension(VideoFile.FileName).ToLowerInvariant();
                var allowedVideoExt = new[] { ".mp4", ".webm", ".ogg", ".mov", ".avi", ".mkv" };
                
                if (!allowedVideoExt.Contains(videoExt))
                {
                    ModelState.AddModelError("VideoFile", "Invalid video type. Allowed: mp4, webm, ogg, mov, avi, mkv.");
                    StatusMessage = "Invalid video file type.";
                    IsSuccess = false;
                    return Page();
                }

                var webroot = _env.WebRootPath;
                var thumbDir = Path.Combine(webroot, "img", "Videos", "Thumb");
                var videoDir = Path.Combine(webroot, "img", "Videos", "Full");
                Directory.CreateDirectory(thumbDir);
                Directory.CreateDirectory(videoDir);

                // Generate unique filename
                var fileName = Guid.NewGuid().ToString("N");
                var thumbFileName = fileName + thumbExt;
                var videoFileName = fileName + videoExt;

                var thumbPath = Path.Combine(thumbDir, thumbFileName);
                var videoPath = Path.Combine(videoDir, videoFileName);

                // Save thumbnail
                using (var fs = System.IO.File.Create(thumbPath))
                {
                    await ThumbnailFile.CopyToAsync(fs);
                }

                // Create thumbnail (max150x150 crop) using ImageSharp
                using (var image = SixLabors.ImageSharp.Image.Load(thumbPath))
                {
                    ImageHelper.SaveImageWithCrop(image, 150, 150, thumbPath);
                }

                // Save video file
                using (var fs = System.IO.File.Create(videoPath))
                {
                    await VideoFile.CopyToAsync(fs);
                }

                var video = new Video
                {
                    Filename = videoFileName,
                    Thumbnail = thumbFileName,
                    Description = Description ?? "",
                    AlbumId = AlbumId,
                    UserId = GetUserId(),
                    Visible = false,
                    Approved = false,
                    Timestamp = DateTime.UtcNow
                };

                _videoService.Add(video);

                StatusMessage = "Video uploaded successfully!";
                IsSuccess = true;
                // Clear form
                AlbumId = 0;
                Description = "";
                ThumbnailFile = null;
                VideoFile = null;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error uploading video: {ex.Message}";
                IsSuccess = false;
            }

            return Page();
        }

        private string GetUserId()
        {
            var email = User?.Identity?.Name;
            return !string.IsNullOrEmpty(email) ? _userHelper.GetUserIdByEmail(email) : null;
        }
    }
}