using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace digioz.Portal.Web.Pages.Shared.Components.SlideShow
{
    public class SlideShowViewComponent : ViewComponent
    {
        private readonly ISlideShowService _slideShowService;
        private readonly IPluginService _pluginService;
        private readonly IMemoryCache _cache;
        private const string CacheKey = "SlideShow_Items";
        private const string PluginCacheKey = "SlideShow_PluginEnabled";

        public SlideShowViewComponent(ISlideShowService slideShowService, IPluginService pluginService, IMemoryCache cache)
        {
            _slideShowService = slideShowService;
            _pluginService = pluginService;
            _cache = cache;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (!_cache.TryGetValue(PluginCacheKey, out bool isEnabled))
            {
                isEnabled = _pluginService.GetAll().Any(p => p.Name == "SlideShow" && p.IsEnabled);
                _cache.Set(PluginCacheKey, isEnabled, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(15)));
            }

            if (!isEnabled)
            {
                return Content(string.Empty);
            }

            if (!_cache.TryGetValue(CacheKey, out List<digioz.Portal.Bo.SlideShow> items))
            {
                items = _slideShowService.GetAll()
                    .OrderByDescending(s => s.DateModified)
                    .ThenByDescending(s => s.DateCreated)
                    .ToList();
                _cache.Set(CacheKey, items, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(15)));
            }

            return View(items);
        }
    }
}
