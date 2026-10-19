using System;
using System.Security.Claims;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using Microsoft.AspNetCore.Http;

namespace digioz.Portal.Web.Services
{
    /// <summary>
    /// Resolves the signed-in user's Profile once per request and reuses it across
    /// view components (UserMenu, BootstrapOverride, etc.) that render on every page.
    /// </summary>
    public interface ICurrentProfileProvider
    {
        string? UserId { get; }
        Profile? GetProfile();
    }

    public sealed class CurrentProfileProvider : ICurrentProfileProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IProfileService _profileService;
        private bool _resolved;
        private Profile? _profile;

        public CurrentProfileProvider(IHttpContextAccessor httpContextAccessor, IProfileService profileService)
        {
            _httpContextAccessor = httpContextAccessor;
            _profileService = profileService;
        }

        private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

        public string? UserId => User?.Identity?.IsAuthenticated == true
            ? User.FindFirstValue(ClaimTypes.NameIdentifier)
            : null;

        public Profile? GetProfile()
        {
            if (_resolved)
            {
                return _profile;
            }

            _resolved = true;

            var userId = UserId;
            if (string.IsNullOrEmpty(userId))
            {
                return null;
            }

            _profile = _profileService.GetByUserId(userId);

            // Fallback: some legacy profiles are only linked by email
            if (_profile == null || string.IsNullOrWhiteSpace(_profile.DisplayName))
            {
                var email = User?.FindFirstValue(ClaimTypes.Email) ?? User?.Identity?.Name;
                if (!string.IsNullOrWhiteSpace(email))
                {
                    var byEmail = _profileService.GetByEmail(email);
                    if (byEmail != null && !string.IsNullOrWhiteSpace(byEmail.DisplayName))
                    {
                        _profile = byEmail;
                    }
                }
            }

            return _profile;
        }
    }
}
