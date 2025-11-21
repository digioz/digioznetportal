using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using digioz.Portal.Utilities;

namespace digioz.Portal.Web.Pages.PrivateMessages
{
    [Authorize]
    public class AddModel : PageModel
    {
        private readonly IPrivateMessageService _pmService;
        private readonly IProfileService _profileService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IMemoryCache _cache;

        private const string UserProfilesCacheKey = "UserProfilesForMessages";

        public AddModel(IPrivateMessageService pmService, IProfileService profileService, UserManager<IdentityUser> userManager, IMemoryCache cache)
        {
            _pmService = pmService;
            _profileService = profileService;
            _userManager = userManager;
            _cache = cache;
        }

        public class UserLite { public string Id { get; set; } = string.Empty; public string DisplayName { get; set; } = string.Empty; }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required]
            public string ToDisplayName { get; set; } = string.Empty; // user types this (Profile.DisplayName)
            public string ToId { get; set; } = string.Empty; // resolved to Identity UserId
            [Required]
            public string Subject { get; set; } = string.Empty;
            [Required]
            public string Message { get; set; } = string.Empty;
        }

        private List<UserLite> GetCachedUserProfiles()
        {
            if (!_cache.TryGetValue(UserProfilesCacheKey, out List<UserLite>? cachedUsers) || cachedUsers == null)
            {
                cachedUsers = _profileService.GetAll()
                    .Where(p => !string.IsNullOrWhiteSpace(p.DisplayName))
                    .Select(p => new UserLite { Id = p.UserId, DisplayName = p.DisplayName })
                    .ToList();
                
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5));
                
                _cache.Set(UserProfilesCacheKey, cachedUsers, cacheOptions);
            }
            
            return cachedUsers;
        }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            var typed = (Input.ToDisplayName ?? string.Empty).Trim();
            if (!string.IsNullOrEmpty(typed))
            {
                var users = GetCachedUserProfiles();

                // First try exact (case-insensitive), then partial contains
                var exact = users.Where(u => string.Equals(u.DisplayName, typed, StringComparison.OrdinalIgnoreCase)).ToList();
                if (exact.Count == 1)
                {
                    Input.ToId = exact[0].Id;
                }
                else if (exact.Count > 1)
                {
                    ModelState.AddModelError(nameof(Input.ToDisplayName), $"Multiple users match '{typed}'. Please refine.");
                }
            }

            var currentUserId = _userManager.GetUserId(User);

            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(Input.ToId))
            {
                if (string.IsNullOrWhiteSpace(Input.ToId))
                {
                    ModelState.AddModelError(nameof(Input.ToDisplayName), "Recipient not found.");
                }
                return Page();
            }
            if (Input.ToId == currentUserId)
            {
                ModelState.AddModelError(nameof(Input.ToDisplayName), "You cannot send a private message to yourself.");
                return Page();
            }

            var sanitizedMessage = StringUtils.SanitizeToPlainText(Input.Message);
            var sanitizedSubject = StringUtils.SanitizeToPlainText(Input.Subject);

            var pm = new PrivateMessage
            {
                FromId = currentUserId,
                ToId = Input.ToId,
                Subject = sanitizedSubject,
                Message = sanitizedMessage
            };
            _pmService.Add(pm);
            return RedirectToPage("/PrivateMessages/Details", new { id = pm.Id });
        }
    }
}
