using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;

namespace digioz.Portal.Web.Areas.Admin.Pages.Theme
{
    public class AddModel : PageModel
    {
        private readonly IThemeService _themeService;

        public AddModel(IThemeService themeService)
        {
            _themeService = themeService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = null!;

        public class InputModel
        {
            [Required]
            [StringLength(200)]
            [Display(Name = "Theme Name")]
            public string Name { get; set; } = string.Empty;

            [Display(Name = "Is Default Theme")]
            public bool IsDefault { get; set; }

            [Required]
            [Display(Name = "CSS Body")]
            public string Body { get; set; } = string.Empty;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // If this theme is being set as default, update all other themes to not be default
            if (Input.IsDefault)
            {
                var allThemes = _themeService.GetAll();
                foreach (var existingTheme in allThemes.Where(t => t.IsDefault))
                {
                    existingTheme.IsDefault = false;
                    _themeService.Update(existingTheme);
                }
            }

            var theme = new Bo.Theme
            {
                Name = Input.Name,
                Body = Input.Body,
                IsDefault = Input.IsDefault,
                CreateDate = DateTime.Now
            };

            _themeService.Add(theme);

            TempData["StatusMessage"] = $"Theme '{theme.Name}' has been created successfully.";
            return RedirectToPage("./Index");
        }
    }
}
