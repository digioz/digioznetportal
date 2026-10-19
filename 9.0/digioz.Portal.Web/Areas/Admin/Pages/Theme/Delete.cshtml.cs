using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;

namespace digioz.Portal.Web.Areas.Admin.Pages.Theme
{
    public class DeleteModel : PageModel
    {
        private readonly IThemeService _themeService;
        private readonly IProfileService _profileService;

        public DeleteModel(IThemeService themeService, IProfileService profileService)
        {
            _themeService = themeService;
            _profileService = profileService;
        }

        [BindProperty]
        public Bo.Theme Theme { get; set; } = null!;

        public bool CannotDelete { get; set; }
        public int UsersAffected { get; set; }

        public IActionResult OnGet(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Theme = _themeService.Get(id.Value);

            if (Theme == null)
            {
                return NotFound();
            }

            // Check if this is the default theme
            if (Theme.IsDefault)
            {
                CannotDelete = true;
                return Page();
            }

            // Count how many users will be affected
            UsersAffected = _profileService.GetAll()
                .Count(p => p.ThemeId == id.Value);

            return Page();
        }

        public IActionResult OnPost()
        {
            if (Theme?.Id == null)
            {
                return NotFound();
            }

            var themeToDelete = _themeService.Get(Theme.Id);

            if (themeToDelete == null)
            {
                return NotFound();
            }

            // Prevent deletion of default theme
            if (themeToDelete.IsDefault)
            {
                TempData["StatusMessage"] = "Cannot delete the default theme. Please set another theme as default first.";
                return RedirectToPage("./Index");
            }

            // Get the default theme to reassign users
            var defaultTheme = _themeService.GetDefault();

            if (defaultTheme == null)
            {
                TempData["StatusMessage"] = "Cannot delete theme: No default theme found. Please ensure at least one theme is set as default.";
                return RedirectToPage("./Index");
            }

            // Update all profiles using this theme to use the default theme instead
            var affectedProfiles = _profileService.GetAll()
                .Where(p => p.ThemeId == Theme.Id)
                .ToList();

            foreach (var profile in affectedProfiles)
            {
                profile.ThemeId = defaultTheme.Id;
                _profileService.Update(profile);
            }

            // Delete the theme
            _themeService.Delete(Theme.Id);

            var usersAffectedCount = affectedProfiles.Count;
            var message = $"Theme '{themeToDelete.Name}' has been deleted successfully.";
            if (usersAffectedCount > 0)
            {
                message += $" {usersAffectedCount} user(s) have been switched to the default theme.";
            }

            TempData["StatusMessage"] = message;
            return RedirectToPage("./Index");
        }
    }
}
