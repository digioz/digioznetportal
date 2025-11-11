using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace digioz.Portal.Web.Pages.Shared.Components.SlideShow
{
    public class SlideShowViewComponent(
        ISlideShowService slideShowService,
        IPluginService pluginService,
        IMemoryCache cache
    ) : ViewComponent
    {
        private const string CacheKey = "SlideShow_Items";
        private const string PluginCacheKey = "SlideShow_PluginEnabled";
        private readonly ISlideShowService _slideShowService = slideShowService;
        private readonly IPluginService _pluginService = pluginService;
        private readonly IMemoryCache _cache = cache;

        public Task<IViewComponentResult> InvokeAsync()
        {
            if (!_cache.TryGetValue(PluginCacheKey, out bool isEnabled))
            {
                isEnabled = _pluginService.GetAll().Any(p => p.Name == "SlideShow" && p.IsEnabled);
                _cache.Set(PluginCacheKey, isEnabled, new MemoryCacheEntryOptions { SlidingExpiration = TimeSpan.FromMinutes(15) });
            }

            if (!isEnabled)
            {
                return Task.FromResult<IViewComponentResult>(Content(string.Empty));
            }

            if (!_cache.TryGetValue(CacheKey, out List<digioz.Portal.Bo.SlideShow>? items) || items == null)
            {
                items = _slideShowService.GetAll()
                    .OrderByDescending(s => s.DateModified)
                    .ThenByDescending(s => s.DateCreated)
                    .ToList();
                _cache.Set(CacheKey, items, new MemoryCacheEntryOptions { SlidingExpiration = TimeSpan.FromMinutes(15) });
            }

            return Task.FromResult<IViewComponentResult>(View(items));
        }
    }
}
