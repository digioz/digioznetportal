using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using digioz.Portal.Dal.Services.Interfaces;
using digioz.Portal.Bo;

namespace digioz.Portal.Web.Areas.Admin.Pages.UserManager
{
    [Authorize(Roles = "Administrator")]
    public class DetailsModel : PageModel
    {
        private readonly IAspNetUserService _userService;
        private readonly IProfileService _profileService;
        private readonly UserManager<IdentityUser> _userManager;

        public DetailsModel(IAspNetUserService userService, IProfileService profileService, UserManager<IdentityUser> userManager)
        {
            _userService = userService;
            _profileService = profileService;
            _userManager = userManager;
        }

        public new AspNetUser? User { get; set; }
        public Profile? Profile { get; set; }
        public System.Collections.Generic.List<string> UserRoles { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            User = _userService.Get(id);
            if (User == null)
            {
                return NotFound();
            }

            Profile = _profileService.GetByUserId(id);
            
            // Get user roles
            var identityUser = await _userManager.FindByIdAsync(id);
            if (identityUser != null)
            {
                UserRoles = (await _userManager.GetRolesAsync(identityUser)).ToList();
            }

            return Page();
        }
    }
}
