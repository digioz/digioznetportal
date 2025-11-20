using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using digioz.Portal.Bo;
using digioz.Portal.Dal.Services.Interfaces;
using System;

namespace digioz.Portal.Web.Pages.PrivateMessages
{
    [Authorize]
    public class AddModel : PageModel
    {
        private readonly IPrivateMessageService _pmService;
        private readonly IProfileService _profileService;
        private readonly UserManager<IdentityUser> _userManager;

        public AddModel(IPrivateMessageService pmService, IProfileService profileService, UserManager<IdentityUser> userManager)
        {
            _pmService = pmService;
            _profileService = profileService;
            _userManager = userManager;
        }

        public class UserLite { public string Id { get; set; } public string DisplayName { get; set; } }

        public List<UserLite> Users { get; set; } = new();

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

        public void OnGet()
        {
            // Pull DisplayName from Profiles
            Users = _profileService.GetAll()
                .Where(p => !string.IsNullOrWhiteSpace(p.DisplayName))
                .Select(p => new UserLite { Id = p.UserId, DisplayName = p.DisplayName })
                .ToList();
        }

        public IActionResult OnPost()
        {
            Users = _profileService.GetAll()
                .Where(p => !string.IsNullOrWhiteSpace(p.DisplayName))
                .Select(p => new UserLite { Id = p.UserId, DisplayName = p.DisplayName })
                .ToList();

            var typed = (Input.ToDisplayName ?? string.Empty).Trim();
            if (!string.IsNullOrEmpty(typed))
            {
                // First try exact (case-insensitive), then partial contains
                var exact = Users.Where(u => string.Equals(u.DisplayName, typed, StringComparison.OrdinalIgnoreCase)).ToList();
                List<UserLite> matches = exact.Count > 0 ? exact : Users.Where(u => u.DisplayName != null && u.DisplayName.Contains(typed, StringComparison.OrdinalIgnoreCase)).ToList();
                if (matches.Count == 1)
                {
                    Input.ToId = matches[0].Id;
                }
                else if (matches.Count > 1)
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

            var pm = new PrivateMessage
            {
                FromId = currentUserId,
                ToId = Input.ToId,
                Subject = Input.Subject,
                Message = Input.Message
            };
            _pmService.Add(pm);
            return RedirectToPage("/PrivateMessages/Details", new { id = pm.Id });
        }
    }
}
