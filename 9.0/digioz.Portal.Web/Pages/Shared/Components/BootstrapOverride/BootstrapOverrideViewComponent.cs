using System.Threading.Tasks;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace digioz.Portal.Web.Pages.Shared.Components.BootstrapOverride
{
    public class BootstrapOverrideViewComponent : ViewComponent
    {
        private readonly IThemeService _themeService;
        private readonly IProfileService _profileService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BootstrapOverrideViewComponent(
            IThemeService themeService,
            IProfileService profileService,
            IHttpContextAccessor httpContextAccessor)
        {
            _themeService = themeService;
            _profileService = profileService;
            _httpContextAccessor = httpContextAccessor;
        }

        public Task<IViewComponentResult> InvokeAsync()
        {
            string? customCss = null;
            
            // Check if the user is logged in
            var user = _httpContextAccessor.HttpContext?.User;
            var isAuthenticated = user?.Identity?.IsAuthenticated ?? false;
            
            if (isAuthenticated && user != null)
            {
                // User is logged in - check for their theme preference
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(userId))
                {
                    var profile = _profileService.GetByUserId(userId);
                    if (profile?.ThemeId != null)
                    {
                        // User has a theme preference
                        var theme = _themeService.Get(profile.ThemeId.Value);
                        if (theme != null)
                        {
                            customCss = theme.Body;
                        }
                    }
                }
            }
            
            // If not logged in, or logged in without theme preference, use default theme
            if (string.IsNullOrEmpty(customCss))
            {
                var defaultTheme = _themeService.GetDefault();
                if (defaultTheme != null)
                {
                    customCss = defaultTheme.Body;
                }
            }

            return Task.FromResult<IViewComponentResult>(View((object?)customCss));
        }
    }
}
