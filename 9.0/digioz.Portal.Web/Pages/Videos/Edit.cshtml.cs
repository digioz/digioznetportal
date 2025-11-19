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

namespace digioz.Portal.Pages.Videos
{
    public class EditModel : PageModel
    {
        private readonly IVideoService _videoService;
        private readonly IVideoAlbumService _albumService;
        private readonly IUserHelper _userHelper;
        private readonly IWebHostEnvironment _env;

        public EditModel(IVideoService videoService, IVideoAlbumService albumService, IUserHelper userHelper, IWebHostEnvironment env)
        {
            _videoService = videoService;
            _albumService = albumService;
            _userHelper = userHelper;
            _env = env;
        }

        public Video? Item { get; private set; }
        public List<VideoAlbum> Albums { get; private set; } = new();
        public bool IsOwner { get; private set; }

        [BindProperty]
        public int AlbumId { get; set; }

        [BindProperty]
        public IFormFile? ThumbnailFile { get; set; }

        [BindProperty]
        public IFormFile? VideoFile { get; set; }

        [BindProperty]
        public string? Description { get; set; }

        public string? StatusMessage { get; set; }
        public bool IsSuccess { get; set; }

        public IActionResult OnGet(int id)
        {
            Item = _videoService.Get(id);
            if (Item == null)
                return NotFound();

            var email = User?.Identity?.Name;
            var userId = !string.IsNullOrEmpty(email) ? _userHelper.GetUserIdByEmail(email) : null;
            IsOwner = Item.UserId == userId;

            bool isAdmin = User?.IsInRole("Admin") == true;
            if (!IsOwner && !isAdmin)
                return Forbid();

            Albums = _albumService.GetAll().OrderBy(a => a.Name).ToList();
            AlbumId = Item.AlbumId;
            Description = Item.Description;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            Item = _videoService.Get(id);
            if (Item == null)
                return NotFound();

            var email = User?.Identity?.Name;
            var userId = !string.IsNullOrEmpty(email) ? _userHelper.GetUserIdByEmail(email) : null;
            IsOwner = Item.UserId == userId;

            bool isAdmin = User?.IsInRole("Admin") == true;
            if (!IsOwner && !isAdmin)
                return Forbid();

            Albums = _albumService.GetAll().OrderBy(a => a.Name).ToList();

            try
            {
                if (Item != null)
                {
                    // Update basic fields
                    Item.AlbumId = AlbumId;
                    Item.Description = Description ?? Item.Description;

                    // Handle thumbnail replacement
                    if (ThumbnailFile != null && ThumbnailFile.Length > 0)
                    {
                        var thumbExt = Path.GetExtension(ThumbnailFile.FileName).ToLowerInvariant();
                        var allowedImageExt = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tif", ".tiff" };

                        if (!allowedImageExt.Contains(thumbExt))
                        {
                            StatusMessage = "Invalid thumbnail image type.";
                            IsSuccess = false;
                            return Page();
                        }

                        var webroot = _env.WebRootPath;
                        var thumbDir = Path.Combine(webroot, "img", "Videos", "Thumb");
                        Directory.CreateDirectory(thumbDir);

                        // Delete old thumbnail
                        var oldThumbPath = Path.Combine(thumbDir, Item.Thumbnail);
                        if (System.IO.File.Exists(oldThumbPath))
                        {
                            System.IO.File.Delete(oldThumbPath);
                        }

                        // Save new thumbnail
                        var fileName = Guid.NewGuid().ToString("N");
                        var thumbFileName = fileName + thumbExt;
                        var thumbPath = Path.Combine(thumbDir, thumbFileName);

                        using (var fs = System.IO.File.Create(thumbPath))
                        {
                            await ThumbnailFile.CopyToAsync(fs);
                        }

                        // Create thumbnail (max150x150 crop)
                        using (var image = SixLabors.ImageSharp.Image.Load(thumbPath))
                        {
                            ImageHelper.SaveImageWithCrop(image, 150, 150, thumbPath);
                        }

                        Item.Thumbnail = thumbFileName;
                    }

                    // Handle video replacement
                    if (VideoFile != null && VideoFile.Length > 0)
                    {
                        var videoExt = Path.GetExtension(VideoFile.FileName).ToLowerInvariant();
                        var allowedVideoExt = new[] { ".mp4", ".webm", ".ogg", ".mov", ".avi", ".mkv" };

                        if (!allowedVideoExt.Contains(videoExt))
                        {
                            StatusMessage = "Invalid video file type.";
                            IsSuccess = false;
                            return Page();
                        }

                        var webroot = _env.WebRootPath;
                        var videoDir = Path.Combine(webroot, "img", "Videos", "Full");
                        Directory.CreateDirectory(videoDir);

                        // Delete old video
                        var oldVideoPath = Path.Combine(videoDir, Item.Filename);
                        if (System.IO.File.Exists(oldVideoPath))
                        {
                            System.IO.File.Delete(oldVideoPath);
                        }

                        // Save new video
                        var fileName = Guid.NewGuid().ToString("N");
                        var videoFileName = fileName + videoExt;
                        var videoPath = Path.Combine(videoDir, videoFileName);

                        using (var fs = System.IO.File.Create(videoPath))
                        {
                            await VideoFile.CopyToAsync(fs);
                        }

                        Item.Filename = videoFileName;
                    }

                    _videoService.Update(Item);
                }

                StatusMessage = "Video updated successfully!";
                IsSuccess = true;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error updating video: {ex.Message}";
                IsSuccess = false;
            }

            return Page();
        }
    }
}
