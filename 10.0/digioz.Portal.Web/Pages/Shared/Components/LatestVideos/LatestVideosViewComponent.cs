using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;

namespace digioz.Portal.Web.Pages.Shared.Components.LatestVideos
{
    public class LatestVideosViewComponent : ViewComponent
    {
        private readonly IVideoService _videoService;
        private readonly IMemoryCache _cache;
        private const string CacheKey = "LatestVideos:6";
        private static readonly MemoryCacheEntryOptions CacheOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(5));

        public LatestVideosViewComponent(IVideoService videoService, IMemoryCache cache)
        {
            _videoService = videoService;
            _cache = cache;
        }

        public Task<IViewComponentResult> InvokeAsync(int count = 6)
        {
            var key = count == 6 ? CacheKey : $"LatestVideos:{count}";
            if (!_cache.TryGetValue(key, out List<Video>? latest) || latest == null)
            {
                latest = _videoService.GetAll()
                    .Where(v => v.Visible && v.Approved)
                    .OrderByDescending(v => v.Timestamp ?? DateTime.MinValue)
                    .ThenByDescending(v => v.Id)
                    .Take(count)
                    .ToList();
                _cache.Set(key, latest, CacheOptions);
            }
            return Task.FromResult<IViewComponentResult>(View(latest));
        }
    }
}
