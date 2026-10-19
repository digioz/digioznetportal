using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.Web.Services;

namespace digioz.Portal.Web.Pages.Shared.Components.UserMenu
{
    public class UserMenuViewComponent(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, ICurrentProfileProvider currentProfileProvider, IPrivateMessageService privateMessageService) : ViewComponent
    {
        private readonly SignInManager<IdentityUser> _signInManager = signInManager;
        private readonly UserManager<IdentityUser> _userManager = userManager;
        private readonly ICurrentProfileProvider _currentProfileProvider = currentProfileProvider;
        private readonly IPrivateMessageService _privateMessageService = privateMessageService;

        public Task<IViewComponentResult> InvokeAsync()
        {
            ViewBag.SignInManager = _signInManager;
            ViewBag.UserManager = _userManager;
            ViewBag.UnreadMessageCount = 0;

            if (_signInManager.IsSignedIn(UserClaimsPrincipal))
            {
                // User id comes straight from the auth cookie claims - no Identity DB round trips needed
                var userId = _currentProfileProvider.UserId;
                if (!string.IsNullOrEmpty(userId))
                {
                    var profile = _currentProfileProvider.GetProfile();
                    if (profile != null && !string.IsNullOrWhiteSpace(profile.DisplayName))
                    {
                        ViewBag.ProfileDisplayName = profile.DisplayName;
                    }

                    ViewBag.UnreadMessageCount = _privateMessageService.GetUnreadCount(userId);
                }
            }

            return Task.FromResult<IViewComponentResult>(View());
        }
    }
}
