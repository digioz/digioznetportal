using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.Utilities.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace digioz.Portal.Web.Areas.Admin.Pages.SlideShow
{
    public class EditModel : PageModel
    {
        private readonly ISlideShowService _service;
        private readonly IWebHostEnvironment _env;

        public EditModel(ISlideShowService service, IWebHostEnvironment env)
        {
            _service = service;
            _env = env;
        }

        [BindProperty] public Bo.SlideShow? Item { get; set; }
        [BindProperty] public new IFormFile? File { get; set; }

        public IActionResult OnGet(string id)
        {
            Item = _service.Get(id);
            if (Item == null) return RedirectToPage("/SlideShow/Index", new { area = "Admin" });
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();
            if (Item == null) return RedirectToPage("/SlideShow/Index", new { area = "Admin" });

            var existingItem = _service.Get(Item.Id);
            if (existingItem == null) return RedirectToPage("/SlideShow/Index", new { area = "Admin" });

            // Update description
            existingItem.Description = Item.Description;
            existingItem.DateModified = DateTime.UtcNow;

            // If a new file is uploaded, process it
            if (File != null && File.Length > 0)
            {
                var ext = Path.GetExtension(File.FileName).ToLowerInvariant();
                var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tif", ".tiff" };
                if (!allowed.Contains(ext))
                {
                    ModelState.AddModelError("File", "Invalid image type. Allowed: jpg, png, gif, bmp, tif, tiff.");
                    Item = existingItem;
                    return Page();
                }

                var webroot = _env.WebRootPath;
                var fullDir = Path.Combine(webroot, "img", "Slides", "Full");
                var thumbDir = Path.Combine(webroot, "img", "Slides", "Thumb");
                Directory.CreateDirectory(fullDir);
                Directory.CreateDirectory(thumbDir);

                var fileName = Guid.NewGuid().ToString("N") + ext;
                var fullPath = Path.Combine(fullDir, fileName);
                var thumbPath = Path.Combine(thumbDir, fileName);

                // Save original full-size image (850x450)
                using (var fs = System.IO.File.Create(fullPath))
                {
                    await File.CopyToAsync(fs);
                }

                // Create full-size slide image using ImageSharp
                using (var image = SixLabors.ImageSharp.Image.Load(fullPath))
                {
                    ImageHelper.SaveImageWithCrop(image, 850, 450, fullPath);
                }

                // Create thumbnail (120x120 crop) using ImageSharp
                using (var thumbImage = SixLabors.ImageSharp.Image.Load(fullPath))
                {
                    ImageHelper.SaveImageWithCrop(thumbImage, 120, 120, thumbPath);
                }

                // Delete old image files if they exist
                if (!string.IsNullOrEmpty(existingItem.Image))
                {
                    var oldFullPath = Path.Combine(fullDir, existingItem.Image);
                    var oldThumbPath = Path.Combine(thumbDir, existingItem.Image);
                    if (System.IO.File.Exists(oldFullPath)) System.IO.File.Delete(oldFullPath);
                    if (System.IO.File.Exists(oldThumbPath)) System.IO.File.Delete(oldThumbPath);
                }

                existingItem.Image = fileName;
            }

            _service.Update(existingItem);
            return RedirectToPage("/SlideShow/Index", new { area = "Admin" });
        }
    }
}
