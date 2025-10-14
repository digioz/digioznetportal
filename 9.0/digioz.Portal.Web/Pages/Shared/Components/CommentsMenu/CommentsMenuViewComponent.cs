using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace digioz.Portal.Web.Pages.Shared.Components.CommentsMenu
{
    public class CommentsMenuViewComponent : ViewComponent
    {
        private readonly ICommentService _commentService;
        private readonly IConfigService _configService;
        private readonly IMemoryCache _cache;
        private const string CacheKeyPrefix = "CommentsMenu_";

        public CommentsMenuViewComponent(ICommentService commentService, IConfigService configService, IMemoryCache cache)
        {
            _commentService = commentService;
            _configService = configService;
            _cache = cache;
        }

        public async Task<IViewComponentResult> InvokeAsync(string referenceId = null)
        {
            var pagePath = HttpContext?.Request?.Path.Value ?? string.Empty;
            if (string.IsNullOrWhiteSpace(pagePath) || pagePath == "/") pagePath = "/Index";

            ViewBag.ReferenceId = referenceId; // optional external id (e.g. page db id)
            ViewBag.ReferenceType = pagePath;   // the logical grouping key

            // Recaptcha config (cached)
            const string cfgCacheKey = "RecaptchaCfg";
            if (!_cache.TryGetValue(cfgCacheKey, out (bool enabled, string publicKey) recaptchaCfg))
            {
                var all = _configService.GetAll();
                var enabledVal = all.FirstOrDefault(c => c.ConfigKey == "RecaptchaEnabled")?.ConfigValue;
                var pub = all.FirstOrDefault(c => c.ConfigKey == "RecaptchaPublicKey")?.ConfigValue;
                bool.TryParse(enabledVal, out var isEnabled);
                recaptchaCfg = (isEnabled, pub ?? string.Empty);
                _cache.Set(cfgCacheKey, recaptchaCfg, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(15)));
            }
            ViewBag.RecaptchaEnabled = recaptchaCfg.enabled;
            ViewBag.RecaptchaPublicKey = recaptchaCfg.publicKey;

            var cacheKey = CacheKeyPrefix + pagePath;
            if (!_cache.TryGetValue(cacheKey, out List<Comment> comments))
            {
                comments = _commentService
                    .GetAll()
                    .Where(c => c.ReferenceType == pagePath)
                    .OrderByDescending(c => c.ModifiedDate ?? c.CreatedDate)
                    .ToList();
                _cache.Set(cacheKey, comments, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(5)));
            }

            return View(comments);
        }
    }
}
