using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;

namespace digioz.Portal.Web.Pages.Shared.Components.LatestPictures
{
    public class LatestPicturesViewComponent : ViewComponent
    {
        private readonly IPictureService _pictureService;
        private readonly IMemoryCache _cache;
        private const string CacheKey = "LatestPictures:6";
        private static readonly MemoryCacheEntryOptions CacheOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(5));

        public LatestPicturesViewComponent(IPictureService pictureService, IMemoryCache cache)
        {
            _pictureService = pictureService;
            _cache = cache;
        }

        public Task<IViewComponentResult> InvokeAsync(int count = 6)
        {
            // Allow overriding count if needed, but keep cache key stable only for default 6
            var key = count == 6 ? CacheKey : $"LatestPictures:{count}";
            if (!_cache.TryGetValue(key, out List<Picture>? latest) || latest == null)
            {
                latest = _pictureService.GetAll()
                    .Where(p => p.Visible && p.Approved)
                    .OrderByDescending(p => p.Timestamp ?? DateTime.MinValue)
                    .ThenByDescending(p => p.Id)
                    .Take(count)
                    .ToList();
                _cache.Set(key, latest, CacheOptions);
            }
            return Task.FromResult<IViewComponentResult>(View(latest));
        }
    }
}
