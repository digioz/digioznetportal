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
        private readonly IProfileService _profileService;
        private readonly IMemoryCache _cache;

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

            ViewBag.ReferenceId = referenceId ?? string.Empty;
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
                    .Where(c => c.ReferenceId == referenceId && c.Visible == true && c.Approved == true)
                    .OrderByDescending(c => c.CreatedDate)
                    .ToList();
            }
            else
            {
                // Get all comments for this reference type
                comments = _commentService.GetByReferenceType(referenceType)
                    .Where(c => c.Visible == true && c.Approved == true)
                    .OrderByDescending(c => c.CreatedDate)
                    .ToList();
            }

            // Build avatar and display name maps (userId -> avatar/DisplayName)
            var avatarMap = new Dictionary<string, string>();
            var displayNameMap = new Dictionary<string, string>();
            
            // Batch load all required profiles to avoid N+1 queries
            var userIds = comments
                .Select(c => c.UserId)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            var profiles = _profileService.GetByUserIds(userIds!)
                .Where(p => p.UserId != null)
                .ToDictionary(p => p.UserId!, p => p);
            
            foreach (var uid in userIds)
            {
                if (profiles.TryGetValue(uid!, out var profile))
                {
                    // Sanitize avatar filename - only store the filename, not any path components
                    if (!string.IsNullOrWhiteSpace(profile.Avatar))
                    {
                        var sanitizedAvatar = System.IO.Path.GetFileName(profile.Avatar.Trim());
                        // Additional validation - only allow safe characters
                        if (!string.IsNullOrWhiteSpace(sanitizedAvatar) && 
                            System.Text.RegularExpressions.Regex.IsMatch(sanitizedAvatar, @"^[a-zA-Z0-9_\-\.]+\.(jpg|jpeg|png|gif|webp)$", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                        {
                            avatarMap[uid!] = sanitizedAvatar;
                        }
                    }
                    
                    // Sanitize display name - trim and limit length
                    if (!string.IsNullOrWhiteSpace(profile.DisplayName))
                    {
                        var sanitizedDisplayName = profile.DisplayName.Trim();
                        // Limit display name length to prevent UI issues
                        if (sanitizedDisplayName.Length > 100)
                        {
                            sanitizedDisplayName = sanitizedDisplayName.Substring(0, 100);
                        }
                        displayNameMap[uid!] = sanitizedDisplayName;
                    }
                }
            }
            
            ViewBag.AvatarMap = avatarMap;
            ViewBag.DisplayNameMap = displayNameMap;

            return Task.FromResult<IViewComponentResult>(View(comments));
        }
    }
}
