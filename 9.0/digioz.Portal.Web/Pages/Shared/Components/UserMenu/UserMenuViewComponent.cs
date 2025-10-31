using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;

namespace digioz.Portal.Web.Pages.Shared.Components.UserMenu
{
    public class UserMenuViewComponent(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager) : ViewComponent
    {
        private readonly SignInManager<IdentityUser> _signInManager = signInManager;
        private readonly UserManager<IdentityUser> _userManager = userManager;

        public Task<IViewComponentResult> InvokeAsync()
        {
            ViewBag.SignInManager = _signInManager;
            ViewBag.UserManager = _userManager;
            return Task.FromResult<IViewComponentResult>(View());
        }
    }
}
