using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    public class EditModel : PageModel
    {
        private readonly IProductService _service;
        private readonly IProductCategoryService _categoryService;
        private readonly IProductOptionService _optionService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public EditModel(
            IProductService service,
            IProductCategoryService categoryService,
            IProductOptionService optionService,
            IWebHostEnvironment webHostEnvironment)
        {
            _service = service;
            _categoryService = categoryService;
            _optionService = optionService;
            _webHostEnvironment = webHostEnvironment;
        }

        [BindProperty] public Bo.Product? Item { get; set; }
        [BindProperty] public IFormFile? ImageFile { get; set; }
        public List<Bo.ProductCategory> Categories { get; set; } = new();
        public List<Bo.ProductOption> ProductOptions { get; set; } = new();
        [BindProperty] public List<Bo.ProductOption>? OptionsData { get; set; }

        public IActionResult OnGet(string id)
        {
            Item = _service.Get(id);
            if (Item == null) return RedirectToPage("/Product/Index", new { area = "Admin" });
            
            Categories = _categoryService.GetAll();
            ProductOptions = _optionService.GetAll().Where(o => o.ProductId == id).ToList();
            
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Categories = _categoryService.GetAll();
                if (Item != null)
                {
                    ProductOptions = _optionService.GetAll().Where(o => o.ProductId == Item.Id).ToList();
                }
                return Page();
            }
            
            if (Item == null) return RedirectToPage("/Product/Index", new { area = "Admin" });
            
            Item.DateModified = DateTime.UtcNow;

            // Set unused fields to null
            Item.Sizes = null;
            Item.Colors = null;
            Item.MaterialType = null;

            // Handle image upload if provided
            if (ImageFile != null && ImageFile.Length > 0)
            {
                await HandleImageUploadAsync();
            }

            _service.Update(Item);

            // Handle ProductOptions
            if (OptionsData != null && OptionsData.Any())
            {
                // Get existing options
                var existingOptions = _optionService.GetAll().Where(o => o.ProductId == Item.Id).ToList();
                
                // Delete options that are no longer in the list
                var optionIdsToKeep = OptionsData.Where(o => !o.Id.StartsWith("new_")).Select(o => o.Id).ToList();
                foreach (var existingOption in existingOptions)
                {
                    if (!optionIdsToKeep.Contains(existingOption.Id))
                    {
                        _optionService.Delete(existingOption.Id);
                    }
                }

                // Add or update options
                foreach (var optionData in OptionsData)
                {
                    if (optionData == null || string.IsNullOrEmpty(optionData.Id) || optionData.Id.StartsWith("new_"))
                    {
                        // New option
                        var newOption = new Bo.ProductOption
                        {
                            Id = Guid.NewGuid().ToString(),
                            ProductId = Item.Id,
                            OptionType = optionData?.OptionType,
                            OptionValue = optionData?.OptionValue
                        };
                        _optionService.Add(newOption);
                    }
                    else
                    {
                        // Update existing option
                        var existingOption = existingOptions.FirstOrDefault(o => o.Id == optionData.Id);
                        if (existingOption != null)
                        {
                            existingOption.OptionType = optionData.OptionType;
                            existingOption.OptionValue = optionData.OptionValue;
                            _optionService.Update(existingOption);
                        }
                    }
                }
            }
            else
            {
                // Delete all options if none are provided
                var existingOptions = _optionService.GetAll().Where(o => o.ProductId == Item.Id).ToList();
                foreach (var option in existingOptions)
                {
                    _optionService.Delete(option.Id);
                }
            }

            return RedirectToPage("/Product/Index", new { area = "Admin" });
        }

        // In HandleImageUploadAsync, add a null check for Item before assigning to Item.Image
        private async Task HandleImageUploadAsync()
        {
            try
            {
                if (ImageFile == null)
                {
                    return;
                }

                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "img", "Products");
                string fullFolder = Path.Combine(uploadsFolder, "Full");
                string thumbFolder = Path.Combine(uploadsFolder, "Thumb");

                // Create directories if they don't exist
                Directory.CreateDirectory(fullFolder);
                Directory.CreateDirectory(thumbFolder);

                // Generate filename - use GUID only (will be under 50 chars)
                string filename = $"{Guid.NewGuid()}{Path.GetExtension(ImageFile.FileName)}";

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
                if (Item != null)
                {
                    Item.Image = filename;
                }
            }
            catch (Exception)
            {
                // Log error if needed, continue without image update
            }
        }
    }
}
