using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.Utilities.Helpers; // ImageHelper / Utility.IsImage
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace digioz.Portal.Pages.Profile
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly IProfileService _profileService;
        private readonly IThemeService _themeService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public EditModel(IProfileService profileService, IThemeService themeService, UserManager<IdentityUser> userManager, IWebHostEnvironment env)
        {
            _profileService = profileService;
            _themeService = themeService;
            _userManager = userManager;
            _env = env;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        [BindProperty]
        public IFormFile? AvatarFile { get; set; }

        public string? StatusMessage { get; set; }
        
        public List<SelectListItem> Themes { get; set; } = new();

        public class InputModel
        {
            public int Id { get; set; }
            public string UserId { get; set; } = string.Empty;
            [Required, StringLength(50, MinimumLength = 3)]
            [Display(Name = "Display Name")]
            public string? DisplayName { get; set; }
            [Required, EmailAddress]
            public string? Email { get; set; }
            public string? FirstName { get; set; }
            public string? MiddleName { get; set; }
            public string? LastName { get; set; }
            public DateTime? Birthday { get; set; }
            public bool? BirthdayVisible { get; set; }
            public string? Address { get; set; }
            public string? Address2 { get; set; }
            public string? City { get; set; }
            public string? State { get; set; }
            public string? Zip { get; set; }
            public string? Country { get; set; }
            public string? Signature { get; set; }
            public string? Avatar { get; set; }
            
            [Display(Name = "Theme")]
            public int? ThemeId { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var profile = _profileService.GetAll().FirstOrDefault(p => p.UserId == user.Id);
            if (profile == null)
            {
                profile = new digioz.Portal.Bo.Profile
                {
                    UserId = user.Id,
                    DisplayName = user.Email,
                    Email = user.Email
                };
                _profileService.Add(profile);
            }

            // Load themes for dropdown
            LoadThemes(profile.ThemeId);

            Input = new InputModel
            {
                Id = profile.Id,
                UserId = profile.UserId ?? string.Empty,
                DisplayName = profile.DisplayName,
                FirstName = profile.FirstName,
                MiddleName = profile.MiddleName,
                LastName = profile.LastName,
                Email = profile.Email,
                Birthday = profile.Birthday,
                BirthdayVisible = profile.BirthdayVisible,
                Address = profile.Address,
                Address2 = profile.Address2,
                City = profile.City,
                State = profile.State,
                Zip = profile.Zip,
                Country = profile.Country,
                Signature = profile.Signature,
                Avatar = profile.Avatar,
                ThemeId = profile.ThemeId
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (!ModelState.IsValid)
            {
                var profile = _profileService.GetAll().FirstOrDefault(p => p.UserId == user.Id);
                LoadThemes(profile?.ThemeId);
                return Page();
            }

            // Trim DisplayName to remove leading and trailing whitespace
            if (!string.IsNullOrEmpty(Input.DisplayName))
            {
                Input.DisplayName = Input.DisplayName.Trim();
            }

            var profileToUpdate = _profileService.GetAll().FirstOrDefault(p => p.Id == Input.Id && p.UserId == user.Id);
            if (profileToUpdate == null)
            {
                ModelState.AddModelError(string.Empty, "Profile not found.");
                LoadThemes(Input.ThemeId);
                return Page();
            }

            if (!string.IsNullOrWhiteSpace(Input.DisplayName) && !string.Equals(Input.DisplayName, profileToUpdate.DisplayName, StringComparison.OrdinalIgnoreCase))
            {
                var exists = _profileService.GetAll().Any(p => p.DisplayName != null && p.DisplayName.Equals(Input.DisplayName, StringComparison.OrdinalIgnoreCase));
                if (exists)
                {
                    ModelState.AddModelError("Input.DisplayName", "Display Name already exists.");
                    LoadThemes(Input.ThemeId);
                    return Page();
                }
            }

            if (!string.IsNullOrWhiteSpace(Input.Email) && !string.Equals(Input.Email, user.Email, StringComparison.OrdinalIgnoreCase))
            {
                user.Email = Input.Email;
                user.UserName = Input.Email;
                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    foreach (var err in updateResult.Errors)
                        ModelState.AddModelError(string.Empty, err.Description);
                    LoadThemes(Input.ThemeId);
                    return Page();
                }
            }

            if (AvatarFile != null && AvatarFile.Length > 0 && digioz.Portal.Utilities.Helpers.Utility.IsImage(AvatarFile))
            {
                var imgRoot = Path.Combine(_env.WebRootPath, "img", "avatar");
                Directory.CreateDirectory(Path.Combine(imgRoot, "Full"));
                Directory.CreateDirectory(Path.Combine(imgRoot, "Thumb"));

                var ext = Path.GetExtension(AvatarFile.FileName);
                var fileName = Guid.NewGuid().ToString("N") + ext;
                var fullPath = Path.Combine(imgRoot, "Full", fileName);
                var thumbPath = Path.Combine(imgRoot, "Thumb", fileName);

                using var ms = new MemoryStream();
                await AvatarFile.CopyToAsync(ms);
                var bytes = ms.ToArray();

                using (var imgFull = Image.Load(bytes))
                {
                    ImageHelper.SaveJigsawImage(imgFull, 800, 800, fullPath);
                }
                using (var imgThumb = Image.Load(bytes))
                {
                    ImageHelper.SaveImageWithCrop(imgThumb, 100, 100, thumbPath);
                }

                if (!string.IsNullOrEmpty(profileToUpdate.Avatar))
                {
                    var oldFull = Path.Combine(imgRoot, "Full", profileToUpdate.Avatar);
                    var oldThumb = Path.Combine(imgRoot, "Thumb", profileToUpdate.Avatar);
                    try { if (System.IO.File.Exists(oldFull)) System.IO.File.Delete(oldFull); } catch { }
                    try { if (System.IO.File.Exists(oldThumb)) System.IO.File.Delete(oldThumb); } catch { }
                }

                profileToUpdate.Avatar = fileName;
            }

            profileToUpdate.DisplayName = Input.DisplayName;
            profileToUpdate.FirstName = Input.FirstName;
            profileToUpdate.MiddleName = Input.MiddleName;
            profileToUpdate.LastName = Input.LastName;
            profileToUpdate.Email = Input.Email;
            profileToUpdate.Birthday = Input.Birthday;
            profileToUpdate.BirthdayVisible = Input.BirthdayVisible;
            profileToUpdate.Address = Input.Address;
            profileToUpdate.Address2 = Input.Address2;
            profileToUpdate.City = Input.City;
            profileToUpdate.State = Input.State;
            profileToUpdate.Zip = Input.Zip;
            profileToUpdate.Country = Input.Country;
            profileToUpdate.Signature = Input.Signature;
            profileToUpdate.ThemeId = Input.ThemeId;

            _profileService.Update(profileToUpdate);

            StatusMessage = "Profile updated successfully.";
            return Redirect($"/Profile/Details?userid={Uri.EscapeDataString(profileToUpdate.DisplayName ?? string.Empty)}");
        }

        private void LoadThemes(int? selectedThemeId)
        {
            var allThemes = _themeService.GetAll().OrderByDescending(t => t.IsDefault).ThenBy(t => t.Name).ToList();
            var defaultTheme = allThemes.FirstOrDefault(t => t.IsDefault);

            // If no theme is selected, default to the default theme
            var effectiveThemeId = selectedThemeId ?? defaultTheme?.Id;

            Themes = allThemes.Select(t => new SelectListItem
            {
                Value = t.Id.ToString(),
                Text = t.IsDefault ? $"{t.Name} (Default)" : t.Name,
                Selected = t.Id == effectiveThemeId
            }).ToList();
        }
    }
}