using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;

namespace digioz.Portal.Web.Areas.Admin.Pages.Theme
{
    public class IndexModel : PageModel
    {
        private readonly IThemeService _themeService;

        public IndexModel(IThemeService themeService)
        {
            _themeService = themeService;
        }

        public List<Bo.Theme> Themes { get; set; } = new();

        [TempData]
        public string? StatusMessage { get; set; }

        public void OnGet()
        {
            Themes = _themeService.GetAll().OrderByDescending(t => t.IsDefault).ThenBy(t => t.Name).ToList();
        }

        public IActionResult OnPostSetDefault(int id)
        {
            var theme = _themeService.Get(id);

            if (theme == null)
            {
                return NotFound();
            }

            _themeService.SetAsDefault(id);

            StatusMessage = $"Theme '{theme.Name}' has been set as the default theme.";
            return RedirectToPage();
        }
    }
}
