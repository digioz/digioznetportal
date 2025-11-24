using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;

namespace digioz.Portal.Web.Areas.Admin.Pages.Theme
{
    public class EditModel : PageModel
    {
        private readonly IThemeService _themeService;

        public EditModel(IThemeService themeService)
        {
            _themeService = themeService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = null!;

        public class InputModel
        {
            public int Id { get; set; }

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

        public IActionResult OnGet(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var theme = _themeService.Get(id.Value);

            if (theme == null)
            {
                return NotFound();
            }

            Input = new InputModel
            {
                Id = theme.Id,
                Name = theme.Name,
                Body = theme.Body,
                IsDefault = theme.IsDefault
            };

            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var theme = _themeService.Get(Input.Id);

            if (theme == null)
            {
                return NotFound();
            }

            // If this theme is being set as default, update all other themes to not be default
            if (Input.IsDefault && !theme.IsDefault)
            {
                var allThemes = _themeService.GetAll();
                foreach (var existingTheme in allThemes.Where(t => t.IsDefault && t.Id != Input.Id))
                {
                    existingTheme.IsDefault = false;
                    _themeService.Update(existingTheme);
                }
            }

            theme.Name = Input.Name;
            theme.Body = Input.Body;
            theme.IsDefault = Input.IsDefault;

            _themeService.Update(theme);

            TempData["StatusMessage"] = $"Theme '{theme.Name}' has been updated successfully.";
            return RedirectToPage("./Index");
        }
    }
}
