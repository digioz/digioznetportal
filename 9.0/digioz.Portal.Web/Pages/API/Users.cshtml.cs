
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using digioz.Portal.Dal.Services.Interfaces;
using System;

namespace digioz.Portal.Web.Pages.API
{
    public class UsersModel : PageModel
    {
        private readonly IProfileService _profileService;

        public UsersModel(IProfileService profileService)
        {
            _profileService = profileService;
        }

        public class UserLite { public string Id { get; set; } public string DisplayName { get; set; } }

        public IActionResult OnGet(string term)
        {
            if (string.IsNullOrEmpty(term) || term.Length < 3)
            {
                return new JsonResult(new List<UserLite>());
            }

            var users = _profileService.GetAll()
                .Where(p => !string.IsNullOrWhiteSpace(p.DisplayName) && p.DisplayName.StartsWith(term, StringComparison.OrdinalIgnoreCase))
                .Select(p => new UserLite { Id = p.UserId, DisplayName = p.DisplayName })
                .ToList();

            return new JsonResult(users);
        }
    }
}
