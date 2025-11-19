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

namespace digioz.Portal.Pages.Pictures
{
    public class EditModel : PageModel
    {
        private readonly IPictureService _pictureService;
        private readonly IPictureAlbumService _albumService;
        private readonly IUserHelper _userHelper;
        private readonly IWebHostEnvironment _env;

        public EditModel(IPictureService pictureService, IPictureAlbumService albumService, IUserHelper userHelper, IWebHostEnvironment env)
        {
            _pictureService = pictureService;
            _albumService = albumService;
            _userHelper = userHelper;
            _env = env;
        }

        [BindProperty]
        public Picture? Item { get; set; }

        // Hide PageModel.File intentionally (file download helper)
        [BindProperty]
        public new IFormFile? File { get; set; }

        [BindProperty]
        public string? Description { get; set; }

        [BindProperty]
        public int AlbumId { get; set; }

        public List<PictureAlbum> Albums { get; private set; } = new();
        public string? StatusMessage { get; set; }
        public bool IsSuccess { get; set; }
        public bool IsOwner { get; set; }

        public IActionResult OnGet(int id)
        {
            Item = _pictureService.Get(id);
            if (Item == null)
                return NotFound();

            var email = User?.Identity?.Name;
            var userId = !string.IsNullOrEmpty(email) ? _userHelper.GetUserIdByEmail(email) : null;
            
            IsOwner = Item.UserId == userId;
            if (!IsOwner && !(User?.IsInRole("Admin") == true))
                return Forbid();

            Albums = _albumService.GetAll().OrderBy(a => a.Name).ToList();
            Description = Item.Description;
            AlbumId = Item.AlbumId;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            Item = _pictureService.Get(id);
            if (Item == null)
                return NotFound();

            var email = User?.Identity?.Name;
            var userId = !string.IsNullOrEmpty(email) ? _userHelper.GetUserIdByEmail(email) : null;
            
            IsOwner = Item.UserId == userId;
            if (!IsOwner && !User?.IsInRole("Admin") == true)
                return Forbid();

            Albums = _albumService.GetAll().OrderBy(a => a.Name).ToList();

            if (AlbumId <= 0)
            {
                ModelState.AddModelError("AlbumId", "Please select an album.");
                StatusMessage = "Please select an album.";
                IsSuccess = false;
                return Page();
            }

            try
            {
                // If a new file is provided, replace the old one
                if (File != null && File.Length > 0 && Item != null)
                {
                    var ext = Path.GetExtension(File.FileName).ToLowerInvariant();
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tif", ".tiff" };
                    
                    if (!allowedExtensions.Contains(ext))
                    {
                        ModelState.AddModelError("File", "Invalid image type. Allowed: jpg, png, gif, bmp, tif, tiff.");
                        StatusMessage = "Invalid image type.";
                        IsSuccess = false;
                        return Page();
                    }

                    var webroot = _env.WebRootPath;
                    var fullDir = Path.Combine(webroot, "img", "Pictures", "Full");
                    var thumbDir = Path.Combine(webroot, "img", "Pictures", "Thumb");

                    // Delete old files
                    try
                    {
                        var oldFullPath = Path.Combine(fullDir, Item.Filename);
                        var oldThumbPath = Path.Combine(thumbDir, Item.Thumbnail);
                        if (System.IO.File.Exists(oldFullPath))
                            System.IO.File.Delete(oldFullPath);
                        if (System.IO.File.Exists(oldThumbPath))
                            System.IO.File.Delete(oldThumbPath);
                    }
                    catch { }

                    Directory.CreateDirectory(fullDir);
                    Directory.CreateDirectory(thumbDir);

                    var fileName = Guid.NewGuid().ToString("N") + ext;
                    var fullPath = Path.Combine(fullDir, fileName);
                    var thumbPath = Path.Combine(thumbDir, fileName);

                    // Save new original
                    using (var fs = System.IO.File.Create(fullPath))
                    {
                        await File.CopyToAsync(fs);
                    }

                    // Create thumbnail
                    using (var image = SixLabors.ImageSharp.Image.Load(fullPath))
                    {
                        ImageHelper.SaveImageWithCrop(image, 150, 150, thumbPath);
                    }

                    Item.Filename = fileName;
                    Item.Thumbnail = fileName;
                }

                if (Item != null)
                {
                    Item.Description = Description ?? Item.Description;
                    Item.AlbumId = AlbumId;
                    Item.Timestamp = DateTime.UtcNow;

                    _pictureService.Update(Item);
                }

                StatusMessage = "Picture updated successfully.";
                IsSuccess = true;
            }
            catch (Exception ex)
            {
                StatusMessage = "Error updating picture: " + ex.Message;
                IsSuccess = false;
            }

            return Page();
        }
    }
}
