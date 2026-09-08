using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;

namespace digioz.Portal.Web.Areas.Admin.Pages.Theme
{
    public class DetailsModel : PageModel
    {
        private readonly IThemeService _themeService;

        public DetailsModel(IThemeService themeService)
        {
            _themeService = themeService;
        }

        public Bo.Theme Theme { get; set; } = null!;

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

            return Page();
        }
    }
}
