using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using digioz.Portal.Dal.Services.Interfaces;

namespace digioz.Portal.Web.Pages.Shared.Components.UserMenu
{
    public class UserMenuViewComponent(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, IProfileService profileService, IPrivateMessageService privateMessageService) : ViewComponent
    {
        private readonly SignInManager<IdentityUser> _signInManager = signInManager;
        private readonly UserManager<IdentityUser> _userManager = userManager;
        private readonly IProfileService _profileService = profileService;
        private readonly IPrivateMessageService _privateMessageService = privateMessageService;

        public async Task<IViewComponentResult> InvokeAsync()
        {
            ViewBag.SignInManager = _signInManager;
            ViewBag.UserManager = _userManager;
            ViewBag.UnreadMessageCount = 0;

            if (_signInManager.IsSignedIn(UserClaimsPrincipal))
            {
                var user = await _userManager.GetUserAsync(UserClaimsPrincipal);
                if (user != null)
                {
                    var userId = await _userManager.GetUserIdAsync(user);
                    var profile = !string.IsNullOrEmpty(userId) ? _profileService.GetByUserId(userId) : null;

                    // Fallback: try email lookup if userId lookup failed or DisplayName missing
                    if (profile == null || string.IsNullOrWhiteSpace(profile.DisplayName))
                    {
                        var email = await _userManager.GetEmailAsync(user);
                        if (!string.IsNullOrWhiteSpace(email))
                        {
                            var byEmail = _profileService.GetByEmail(email);
                            if (byEmail != null && !string.IsNullOrWhiteSpace(byEmail.DisplayName))
                            {
                                profile = byEmail;
                            }
                        }
                    }

                    if (profile != null && !string.IsNullOrWhiteSpace(profile.DisplayName))
                    {
                        ViewBag.ProfileDisplayName = profile.DisplayName;
                    }

                    // Get unread private message count
                    if (!string.IsNullOrEmpty(userId))
                    {
                        ViewBag.UnreadMessageCount = _privateMessageService.GetUnreadCount(userId);
                    }
                }
            }

            return View();
        }
    }
}
