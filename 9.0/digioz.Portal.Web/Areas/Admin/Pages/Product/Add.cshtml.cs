using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace digioz.Portal.Web.Areas.Admin.Pages.Product
{
    public class AddModel : PageModel
    {
        private readonly IProductService _service;
        private readonly IProductCategoryService _categoryService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        
        public AddModel(IProductService service, IProductCategoryService categoryService, IWebHostEnvironment webHostEnvironment)
        {
            _service = service;
            _categoryService = categoryService;
            _webHostEnvironment = webHostEnvironment;
        }

        [BindProperty] public Bo.Product Item { get; set; } = new Bo.Product 
        { 
            Visible = true, 
            Id = Guid.NewGuid().ToString(),
            Price = 0,
            DateCreated = DateTime.UtcNow,
            DateModified = DateTime.UtcNow
        };

        [BindProperty] public IFormFile? ImageFile { get; set; }

        public List<Bo.ProductCategory> Categories { get; set; } = new();

        public void OnGet()
        {
            Categories = _categoryService.GetAll();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Categories = _categoryService.GetAll();
                return Page();
            }
            
            if (string.IsNullOrEmpty(Item.Id))
            {
                Item.Id = Guid.NewGuid().ToString();
            }
            
            Item.DateCreated = DateTime.UtcNow;
            Item.DateModified = DateTime.UtcNow;

            // Set unused fields to null
            Item.Sizes = null;
            Item.Colors = null;
            Item.MaterialType = null;
            
            // Handle image upload
            if (ImageFile != null && ImageFile.Length > 0)
            {
                await HandleImageUploadAsync();
            }
            
            _service.Add(Item);
            return RedirectToPage("/Product/Index", new { area = "Admin" });
        }

        private async Task HandleImageUploadAsync()
        {
            try
            {
                if (ImageFile == null || ImageFile.Length == 0)
                {
                    Item.Image = null;
                    return;
                }

                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "img", "Products");
                string fullFolder = Path.Combine(uploadsFolder, "Full");
                string thumbFolder = Path.Combine(uploadsFolder, "Thumb");

                // Create directories if they don't exist
                Directory.CreateDirectory(fullFolder);
                Directory.CreateDirectory(thumbFolder);

                // Generate filename - use GUID only (will be under 50 chars)
                string extension = ".jpg";
                if (!string.IsNullOrEmpty(ImageFile.FileName))
                {
                    extension = Path.GetExtension(ImageFile.FileName);
                }
                string filename = $"{Guid.NewGuid()}{extension}";

                // Save full size image
                string fullFilePath = Path.Combine(fullFolder, filename);
                using (var stream = ImageFile.OpenReadStream())
                {
                    using (var image = await Image.LoadAsync(stream))
                    {
                        await image.SaveAsync(fullFilePath);
                    }
                }

                // Create and save thumbnail (200x200)
                string thumbFilePath = Path.Combine(thumbFolder, filename);
                using (var stream = ImageFile.OpenReadStream())
                {
                    using (var image = await Image.LoadAsync(stream))
                    {
                        image.Mutate(x => x.Resize(new ResizeOptions
                        {
                            Size = new Size(200, 200),
                            Mode = ResizeMode.Max
                        }));
                        await image.SaveAsync(thumbFilePath);
                    }
                }

                // Store only the filename, not the full path
                Item.Image = filename;
            }
            catch (Exception)
            {
                // Log error if needed, continue without image
                Item.Image = null;
            }
        }
    }
}
