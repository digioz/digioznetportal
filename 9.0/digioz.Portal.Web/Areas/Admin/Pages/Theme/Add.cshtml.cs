using System;
using System.ComponentModel.DataAnnotations;
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

            [Display(Name = "CSS Body")]
            public string? Body { get; set; }
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

            var theme = new Bo.Theme
            {
                Name = Input.Name,
                Body = Input.Body ?? string.Empty,
                IsDefault = Input.IsDefault,
                CreateDate = DateTime.Now
            };

            _themeService.Add(theme);

            // If this theme is being set as default, use the service method
            if (Input.IsDefault)
            {
                _themeService.SetAsDefault(theme.Id);
            }

            TempData["StatusMessage"] = $"Theme '{theme.Name}' has been created successfully.";
            return RedirectToPage("./Index");
        }
    }
}
