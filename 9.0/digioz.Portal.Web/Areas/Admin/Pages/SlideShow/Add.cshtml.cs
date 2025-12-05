using System;
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

namespace digioz.Portal.Web.Areas.Admin.Pages.SlideShow
{
    public class AddModel : PageModel
    {
        private readonly ISlideShowService _service;
        private readonly IWebHostEnvironment _env;

        public AddModel(ISlideShowService service, IWebHostEnvironment env)
        {
            _service = service;
            _env = env;
        }

        [BindProperty] public Bo.SlideShow Item { get; set; } = new Bo.SlideShow();
        [BindProperty] public new IFormFile? File { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
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

            Item.Id = Guid.NewGuid().ToString();
            Item.Image = fileName;
            Item.DateCreated = DateTime.UtcNow;
            Item.DateModified = DateTime.UtcNow;

            _service.Add(Item);
            return RedirectToPage("/SlideShow/Index", new { area = "Admin" });
        }
    }
}
