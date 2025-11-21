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
        private readonly IProfileService _profileService; // added for avatar and display name lookup
        private readonly IMemoryCache _cache; // still used for Recaptcha only

        public CommentsMenuViewComponent(ICommentService commentService, IConfigService configService, IProfileService profileService, IMemoryCache cache)
        {
            _commentService = commentService;
            _configService = configService;
            _profileService = profileService;
            _cache = cache;
        }

        public Task<IViewComponentResult> InvokeAsync(string? referenceId = null, string? referenceType = null)
        {
            // If referenceType is not provided, use the current page path
            if (string.IsNullOrWhiteSpace(referenceType))
            {
                var pagePath = HttpContext?.Request?.Path.Value ?? string.Empty;
                if (string.IsNullOrWhiteSpace(pagePath) || pagePath == "/") pagePath = "/Index";
                referenceType = pagePath;
            }

            ViewBag.ReferenceId = referenceId;
            ViewBag.ReferenceType = referenceType;

            // Recaptcha config (cache OK, low churn)
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

            // Get comments filtered by both ReferenceType and ReferenceId
            List<Comment> comments;
            if (!string.IsNullOrWhiteSpace(referenceId))
            {
                // Get comments for specific reference (e.g., specific announcement)
                comments = _commentService.GetByReferenceType(referenceType)
                    .Where(c => c.ReferenceId == referenceId)
                    .ToList();
            }
            else
            {
                // Get all comments for this reference type
                comments = _commentService.GetByReferenceType(referenceType);
            }

            // Build avatar and display name maps (userId -> avatar/DisplayName)
            var avatarMap = new Dictionary<string, string>();
            var displayNameMap = new Dictionary<string, string>();
            foreach (var uid in comments.Select(c => c.UserId).Where(id => !string.IsNullOrWhiteSpace(id)).Distinct())
            {
                var profile = _profileService.GetByUserId(uid!);
                if (profile != null)
                {
                    if (!string.IsNullOrWhiteSpace(profile.Avatar))
                    {
                        avatarMap[uid!] = profile.Avatar.Trim();
                    }
                    if (!string.IsNullOrWhiteSpace(profile.DisplayName))
                    {
                        displayNameMap[uid!] = profile.DisplayName.Trim();
                    }
                }
            }
            ViewBag.AvatarMap = avatarMap; // consumed by view
            ViewBag.DisplayNameMap = displayNameMap; // consumed by view

            return Task.FromResult<IViewComponentResult>(View(comments));
        }
    }
}
