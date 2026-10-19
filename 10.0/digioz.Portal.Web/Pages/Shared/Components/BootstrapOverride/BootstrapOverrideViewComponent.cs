using System;
using System.Threading.Tasks;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace digioz.Portal.Web.Pages.Shared.Components.BootstrapOverride
{
    public class BootstrapOverrideViewComponent : ViewComponent
    {
        private static readonly TimeSpan ThemeCacheDuration = TimeSpan.FromMinutes(15);

        private readonly IThemeService _themeService;
        private readonly ICurrentProfileProvider _currentProfileProvider;
        private readonly IMemoryCache _cache;

        public BootstrapOverrideViewComponent(
            IThemeService themeService,
            ICurrentProfileProvider currentProfileProvider,
            IMemoryCache cache)
        {
            _themeService = themeService;
            _currentProfileProvider = currentProfileProvider;
            _cache = cache;
        }

        public Task<IViewComponentResult> InvokeAsync()
        {
            string? customCss = null;

            // Profile is resolved once per request and shared with the other view components
            var profile = _currentProfileProvider.GetProfile();
            if (profile?.ThemeId != null)
            {
                var theme = GetTheme(profile.ThemeId.Value);
                if (theme != null)
                {
                    customCss = theme.Body;
                }
            }

            // If not logged in, or logged in without theme preference, use default theme
            if (string.IsNullOrEmpty(customCss))
            {
                var defaultTheme = GetDefaultTheme();
                if (defaultTheme != null)
                {
                    customCss = defaultTheme.Body;
                }
            }

            var sanitizedCss = string.IsNullOrWhiteSpace(customCss)
                ? customCss
                : customCss.Replace("</style", "<\\/style", StringComparison.OrdinalIgnoreCase);

            return Task.FromResult<IViewComponentResult>(View((object?)sanitizedCss));
        }

        private Theme? GetTheme(int id)
        {
            return _cache.GetOrCreate(CacheKeys.Theme(id), entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = ThemeCacheDuration;
                return _themeService.Get(id);
            });
        }

        private Theme? GetDefaultTheme()
        {
            return _cache.GetOrCreate(CacheKeys.DefaultTheme, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = ThemeCacheDuration;
                return _themeService.GetDefault();
            });
        }
    }
}
